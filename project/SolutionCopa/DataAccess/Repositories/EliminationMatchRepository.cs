using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

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

    public IReadOnlyCollection<Match> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchElimination>()
            .Cast<Match>()
            .ToList();
    }

    public void Add(Match match)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        MatchEf.Add(context, match);
    }

    public void Update(Match match)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        MatchEf.Update(context, match);
    }

    public void Delete(Match match)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        MatchEf.Delete(context, match.MatchId);
    }

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
