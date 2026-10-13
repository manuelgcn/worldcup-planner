using WorldCupPlanner.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

// Operaciones EF Core compartidas por los tres repositorios que viven sobre la tabla TPH "Matches"
// (MatchRepository, GroupMatchRepository, EliminationMatchRepository). Centraliza las dos piezas
// delicadas de este hito: el attach de grafos detached y los includes con identity resolution.
// Las escrituras reciben la factory y gestionan su propio contexto efimero (autosave por operacion);
// las lecturas exponen WithGraph para que cada repo aplique su filtro tipado (OfType/Where).
internal static class MatchEf
{
    // Carga el grafo completo de un partido con identity resolution, de modo que las cadenas
    // autorreferentes del bracket (Final -> Semis -> ...) apunten a las MISMAS instancias dentro
    // del resultado y GetWinner() resuelva a traves de la cadena. Un nivel de include para
    // MatchHome/MatchAway alcanza porque GetAll trae la coleccion completa.
    public static IQueryable<Match> WithGraph(IQueryable<Match> matches) =>
        matches
            .AsNoTrackingWithIdentityResolution()
            .Include(match => match.HomeTeam)
            .Include(match => match.AwayTeam)
            .Include(match => match.Stadium)
            .Include(match => ((MatchGroup)match).Group!).ThenInclude(group => group.Teams)
            .Include(match => ((MatchGroup)match).Journey)
            .Include(match => ((MatchElimination)match).MatchHome)
            .Include(match => ((MatchElimination)match).MatchAway)
            .Include(match => match.Incidents).ThenInclude(incident => incident.Team);

    // Inserta SOLO la fila del partido. Las entidades relacionadas (equipos, estadio, grupo, jornada
    // y partidos fuente) ya estan persistidas: se attachean como Unchanged ANTES del Add para que EF
    // no las marque Added (lo que duplicaria PKs). Idempotente ante un MatchId ya existente.
    public static void Add(IDbContextFactory<AppDbContext> contextFactory, Match match)
    {
        using AppDbContext context = contextFactory.CreateDbContext();
        if (context.Matches.Any(stored => stored.MatchId == match.MatchId))
        {
            return;
        }

        AttachRelated(context, match);
        context.Add(match);
        context.SaveChanges();
    }

    // Copia los escalares editables sobre la instancia trackeada (incluye los derivados de TPH:
    // Phase, MatchCode, penales, WasSimulated) y re-apunta el unico FK que una edicion puede cambiar:
    // el estadio. NO usa context.Update(match), que re-trackearia el grafo entero y caeria en las
    // mismas trampas del Add. Si el partido no existe, lo inserta (conserva la semantica previa).
    public static void Update(IDbContextFactory<AppDbContext> contextFactory, Match match)
    {
        using AppDbContext context = contextFactory.CreateDbContext();
        Match? existing = context.Matches.FirstOrDefault(stored => stored.MatchId == match.MatchId);
        if (existing is null)
        {
            AttachRelated(context, match);
            context.Add(match);
            context.SaveChanges();
            return;
        }

        context.Entry(existing).CurrentValues.SetValues(match);
        if (match.Stadium is not null)
        {
            context.Entry(existing).Property("StadiumId").CurrentValue = match.Stadium.Id;
        }
        SyncIncidents(context, match);
        context.SaveChanges();
    }

    // Sincroniza la coleccion de incidencias por diff de Ids contra lo almacenado: borra las que ya
    // no estan en la instancia editada y agrega las nuevas (con su Team attacheado como Unchanged y
    // el shadow FK MatchId seteado a mano, porque la instancia trackeada no expone la coleccion mutable).
    private static void SyncIncidents(AppDbContext context, Match match)
    {
        List<MatchIncident> stored = context.Set<MatchIncident>()
            .Where(incident => EF.Property<int?>(incident, "MatchId") == match.MatchId)
            .ToList();

        foreach (MatchIncident removed in stored.Where(s => match.Incidents.All(i => i.Id != s.Id)))
        {
            context.Remove(removed);
        }

        foreach (MatchIncident added in match.Incidents.Where(i => stored.All(s => s.Id != i.Id)))
        {
            MarkUnchanged(context, added.Team);
            context.Add(added);
            context.Entry(added).Property("MatchId").CurrentValue = match.MatchId;
        }
    }

    public static void Delete(IDbContextFactory<AppDbContext> contextFactory, int matchId)
    {
        using AppDbContext context = contextFactory.CreateDbContext();
        Match? existing = context.Matches.FirstOrDefault(stored => stored.MatchId == matchId);
        if (existing is not null)
        {
            context.Matches.Remove(existing);
            context.SaveChanges();
        }
    }

    private static void AttachRelated(AppDbContext context, Match match)
    {
        MarkUnchanged(context, match.HomeTeam);
        MarkUnchanged(context, match.AwayTeam);
        MarkUnchanged(context, match.Stadium);

        if (match is MatchGroup groupMatch)
        {
            MarkUnchanged(context, groupMatch.Group);
            MarkUnchanged(context, groupMatch.Journey);
        }

        if (match is MatchElimination eliminationMatch)
        {
            MarkSourceMatchUnchanged(context, eliminationMatch.MatchHome);
            MarkSourceMatchUnchanged(context, eliminationMatch.MatchAway);
        }
    }

    // Defensa en profundidad. Las fuentes de un partido de eliminacion (MatchHome/MatchAway) deben
    // estar YA persistidas: la app las crea y guarda en la ronda previa antes de generar la
    // siguiente. La ronda inicial (R32) no tiene fuentes (null) y se omite. Si llegara una fuente NO
    // persistida, fallamos rapido y claro -con el MatchId ofensor- en vez de marcarla Unchanged y
    // dejar que el INSERT salga con un FK colgante que el motor rechazaria con una violacion de FK
    // dificil de diagnosticar (justo el sintoma que ocultaba el provider InMemory).
    private static void MarkSourceMatchUnchanged(AppDbContext context, Match? sourceMatch)
    {
        if (sourceMatch is null)
        {
            return;
        }

        if (!context.Matches.Any(stored => stored.MatchId == sourceMatch.MatchId))
        {
            throw new MatchPersistenceException(
                $"No se puede persistir el partido de eliminacion: el partido fuente {sourceMatch.MatchId} no existe en la base.");
        }

        MarkUnchanged(context, sourceMatch);
    }

    // Attachea UNA entidad ya persistida sin recorrer su grafo. Usar Entry().State (en vez de
    // Attach/Add) es clave: no traversa navegaciones, asi evita el conflicto "another instance with
    // the same key is already tracked" cuando un partido de eliminacion y sus fuentes traen el mismo
    // estadio/equipo en instancias distintas (vienen de contextos/lecturas diferentes).
    private static void MarkUnchanged(AppDbContext context, object? entity)
    {
        if (entity is null)
        {
            return;
        }

        if (context.Entry(entity).State == EntityState.Detached)
        {
            context.Entry(entity).State = EntityState.Unchanged;
        }
    }
}
