using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// Operaciones EF Core compartidas por los tres repositorios que viven sobre la tabla TPH "Matches"
// (MatchRepository, GroupMatchRepository, EliminationMatchRepository). Centraliza las dos piezas
// delicadas de este hito: el attach de grafos detached y los includes con identity resolution.
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
            .Include(match => ((MatchElimination)match).MatchAway);

    // Inserta SOLO la fila del partido. Las entidades relacionadas (equipos, estadio, grupo, jornada
    // y partidos fuente) ya estan persistidas: se attachean como Unchanged ANTES del Add para que EF
    // no las marque Added (lo que duplicaria PKs). Idempotente ante un MatchId ya existente.
    public static void Add(AppDbContext context, Match match)
    {
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
    public static void Update(AppDbContext context, Match match)
    {
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
        context.SaveChanges();
    }

    public static void Delete(AppDbContext context, int matchId)
    {
        Match? existing = context.Matches.FirstOrDefault(stored => stored.MatchId == matchId);
        if (existing is not null)
        {
            context.Matches.Remove(existing);
            context.SaveChanges();
        }
    }

    public static void AttachRelated(AppDbContext context, Match match)
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
            MarkUnchanged(context, eliminationMatch.MatchHome);
            MarkUnchanged(context, eliminationMatch.MatchAway);
        }
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
