using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockGroupRepository : IGroupRepository
{
    private readonly Dictionary<int, Group> _groups = new Dictionary<int, Group>();

    public void Seed(int groupId, Group group)
    {
        _groups[groupId] = group;
    }

    public Group? GetById(int groupId)
    {
        return _groups.GetValueOrDefault(groupId);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        return _groups.Values.ToList();
    }
}
