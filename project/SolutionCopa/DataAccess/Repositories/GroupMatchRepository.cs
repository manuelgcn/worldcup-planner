using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// Vista tipada de los partidos de grupo sobre la tabla TPH "Matches" (OfType<MatchGroup>).
// Solo se usa en DataAccessTest; se migra a EF para no dejar almacenamiento List<T> en DataAccess.
public class GroupMatchRepository : IGroupMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public GroupMatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Match? GetById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchGroup>()
            .Cast<Match>()
            .FirstOrDefault(match => match.MatchId == id);
    }

    public IReadOnlyCollection<Match> GetByGroup(string groupName)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchGroup>()
            .Where(match => match.Group != null && match.Group.Name == groupName)
            .Cast<Match>()
            .ToList();
    }

    public IReadOnlyCollection<Match> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return MatchEf.WithGraph(context.Matches)
            .OfType<MatchGroup>()
            .Cast<Match>()
            .ToList();
    }

    public void Add(Match match) => MatchEf.Add(_contextFactory, match);

    public void Update(Match match) => MatchEf.Update(_contextFactory, match);

    public void Delete(Match match) => MatchEf.Delete(_contextFactory, match.MatchId);

    public bool ExistsById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Matches.AsNoTracking().OfType<MatchGroup>().Any(match => match.MatchId == id);
    }
}
