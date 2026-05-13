using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly List<Group> _groups = new();

    public void Add(Group group)
    {
        _groups.Add(group);
    }

    public void Update(Group group)
    {
        int index = _groups.FindIndex(existingGroup => existingGroup.Name == group.Name);

        if (index >= 0)
        {
            _groups[index] = group;
            return;
        }

        _groups.Add(group);
    }

    public void Delete(Group group)
    {
        _groups.RemoveAll(existingGroup => existingGroup.Name == group.Name);
    }

    public Group? GetByName(string name)
    {
        return _groups.FirstOrDefault(group => group.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _groups.Any(group => group.Name == name);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        return _groups.AsReadOnly();
    }
}
