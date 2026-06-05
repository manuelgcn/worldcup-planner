using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// RED: esqueleto sobre IDbContextFactory. La implementacion EF Core (OfType<MatchElimination>) llega en GREEN.
public class EliminationMatchRepository : IEliminationMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EliminationMatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Match? GetById(int id) => throw new NotImplementedException();

    public IReadOnlyCollection<Match> GetByPhase(Phase phase) => throw new NotImplementedException();

    public IReadOnlyCollection<Match> GetAll() => throw new NotImplementedException();

    public void Add(Match match) => throw new NotImplementedException();

    public void Update(Match match) => throw new NotImplementedException();

    public void Delete(Match match) => throw new NotImplementedException();

    public bool ExistsById(int id) => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();
}
