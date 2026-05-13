using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockGroupRepository : IGroupRepository
{
    private readonly Dictionary<string, Group> _groups = new();
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Seed(int groupId, Group group)
    {
        _groups[group.Name] = group;
    }

    public void Add(Group group)
    {
        _groups[group.Name] = group;
        AddCalls++;
    }

    public void Update(Group group)
    {
        _groups[group.Name] = group;
        UpdateCalls++;
    }

    public void Delete(Group group)
    {
        _groups.Remove(group.Name);
        DeleteCalls++;
    }

    public Group? GetByName(string name)
    {
        return _groups.GetValueOrDefault(name);
    }

    public bool ExistsByName(string name)
    {
        return _groups.ContainsKey(name);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        return _groups.Values.ToList();
    }

    public void Clear()
    {
        _groups.Clear();
    }
}
