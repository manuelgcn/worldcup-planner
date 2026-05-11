using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockGroupRepository : IGroupRepository
{
    private readonly Dictionary<int, Group> _groups = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Add(Group group)
    {
        _groups[group.Id] = group;
        AddCalls++;
    }

    public void Update(Group group)
    {
        _groups[group.Id] = group;
        UpdateCalls++;
    }

    public void Delete(Group group)
    {
        _groups.Remove(group.Id);
        DeleteCalls++;
    }

    public Group? GetById(int groupId)
    {
        return _groups.GetValueOrDefault(groupId);
    }

    public Group? GetByName(string name)
    {
        return _groups.Values.FirstOrDefault(group => group.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _groups.Values.Any(group => group.Name == name);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        return _groups.Values.ToList();
    }
}
