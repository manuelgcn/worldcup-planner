using ProjectCopa;

namespace Services.Interfaces;

public interface IGroupService
{
    IReadOnlyCollection<Group> GetAllGroups();
    void AddGroup(Group group);
    Group GetByName(string name);
    void RemoveByName(string name);
}
