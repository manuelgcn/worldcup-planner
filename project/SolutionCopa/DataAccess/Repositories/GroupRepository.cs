using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// RED: esqueleto sobre IDbContextFactory. La implementacion EF Core llega en el commit GREEN.
public class GroupRepository : IGroupRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public GroupRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Group group) => throw new NotImplementedException();

    public void Update(Group group) => throw new NotImplementedException();

    public void Delete(Group group) => throw new NotImplementedException();

    public Group? GetByName(string name) => throw new NotImplementedException();

    public bool ExistsByName(string name) => throw new NotImplementedException();

    public IReadOnlyCollection<Group> GetAll() => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();
}
