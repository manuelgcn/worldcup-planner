using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

// Vista tipada de los partidos de eliminacion sobre la tabla TPH "Matches" (OfType<MatchElimination>).
public class EliminationMatchRepository : IEliminationMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EliminationMatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Match? GetById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchElimination>()
            .Cast<Match>()
            .FirstOrDefault(match => match.MatchId == id);
    }

    public IReadOnlyCollection<Match> GetByPhase(Phase phase)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchElimination>()
            .Where(match => match.Phase == phase)
            .Cast<Match>()
            .ToList();
    }

    public bool Any()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.OfType<MatchElimination>().Any();
    }

    public int? MaxMatchId()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.OfType<MatchElimination>().Max(match => (int?)match.MatchId);
    }

    // Solo las dos FK de las filas que tienen fuente (la primera ronda no tiene): sin equipos, estadios ni incidencias.
    public IReadOnlySet<int> GetSourceMatchIds()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.OfType<MatchElimination>()
            .Select(match => new { Home = EF.Property<int?>(match, "MatchHomeId"), Away = EF.Property<int?>(match, "MatchAwayId") })
            .Where(sources => sources.Home != null || sources.Away != null)
            .AsEnumerable()
            .SelectMany(sources => new[] { sources.Home, sources.Away })
            .OfType<int>()
            .ToHashSet();
    }

    public IReadOnlyCollection<Match> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchElimination>()
            .Cast<Match>()
            .ToList();
    }

    public void Add(Match match) => MatchEf.Add(_contextFactory, match);

    public void Update(Match match) => MatchEf.Update(_contextFactory, match);

    public void Delete(Match match) => MatchEf.Delete(_contextFactory, match.MatchId);

    public bool ExistsById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.AsNoTracking().OfType<MatchElimination>().Any(match => match.MatchId == id);
    }

    public void Clear()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        // Solo borra los partidos de eliminacion; los de grupo (fuentes) permanecen. Cargar a tracking
        // permite que ClientSetNull nullee las autorreferencias entre rondas antes de borrar.
        List<MatchElimination> eliminations = context.Set<MatchElimination>().ToList();
        context.RemoveRange(eliminations);
        context.SaveChanges();
    }
}
