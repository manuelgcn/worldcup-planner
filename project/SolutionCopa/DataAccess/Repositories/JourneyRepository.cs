using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// RED: esqueleto sobre IDbContextFactory. La implementacion EF Core llega en el commit GREEN.
public class JourneyRepository : IJourneyRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public JourneyRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Journey journey) => throw new NotImplementedException();

    public void Update(Journey journey) => throw new NotImplementedException();

    public void Delete(Journey journey) => throw new NotImplementedException();

    public Journey? GetByNumber(int number) => throw new NotImplementedException();

    public bool ExistsByNumber(int number) => throw new NotImplementedException();

    public IReadOnlyCollection<Journey> GetAll() => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();
}
