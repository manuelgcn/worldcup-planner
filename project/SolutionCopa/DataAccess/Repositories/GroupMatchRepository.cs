using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// RED: esqueleto sobre IDbContextFactory. La implementacion EF Core llega en el commit GREEN.
// Solo se usa en DataAccessTest (no esta registrado en produccion), pero se migra para que no
// quede ningun almacenamiento List<T> en DataAccess.
public class GroupMatchRepository : IGroupMatchRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public GroupMatchRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Match? GetById(int id) => throw new NotImplementedException();

    public IReadOnlyCollection<Match> GetByGroup(string groupName) => throw new NotImplementedException();

    public IReadOnlyCollection<Match> GetAll() => throw new NotImplementedException();

    public void Add(Match match) => throw new NotImplementedException();

    public void Update(Match match) => throw new NotImplementedException();

    public void Delete(Match match) => throw new NotImplementedException();

    public bool ExistsById(int id) => throw new NotImplementedException();
}
