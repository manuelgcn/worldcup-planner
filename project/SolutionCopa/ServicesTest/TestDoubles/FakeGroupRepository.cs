using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeGroupRepository : IGroupRepository
{
    private readonly Dictionary<int, IGroup> _groups = new();

    public void Set(int id, IGroup group)
    {
        _groups[id] = group;
    }

    public IGroup? GetById(int groupId)
    {
        IGroup? group;
        var found = _groups.TryGetValue(groupId, out group);
        if (!found)
        {
            return null;
        }

        return group;
    }
}
