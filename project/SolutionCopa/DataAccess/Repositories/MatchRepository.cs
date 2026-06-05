using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// RED: esqueleto sobre IDbContextFactory. La implementacion EF Core (TPH) llega en el commit GREEN.
public class MatchRepository : IMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public bool ExistsById(int matchId) => throw new NotImplementedException();

    public void Add(Match match) => throw new NotImplementedException();

    public void Update(Match match) => throw new NotImplementedException();

    public void Delete(Match match) => throw new NotImplementedException();

    public Match? GetById(int matchId) => throw new NotImplementedException();

    public List<Match> GetByGroup(string groupName) => throw new NotImplementedException();

    public List<Match> GetByJourney(int journeyNumber) => throw new NotImplementedException();

    public List<Match> GetByStadium(int stadiumId) => throw new NotImplementedException();

    public List<Match> GetByPhase(Phase phase) => throw new NotImplementedException();

    public List<Match> GetByDateRange(DateTime from, DateTime to) => throw new NotImplementedException();

    public List<Match> GetAll() => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();
}
