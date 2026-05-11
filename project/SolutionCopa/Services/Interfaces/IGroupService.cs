using ProjectCopa;

namespace Services.Interfaces;

public interface IGroupService
{
    IReadOnlyCollection<Group> GetAllGroups();
    void AddGroup(Group group);
    Group GetById(int id);
    Group GetByName(string name);
    void RemoveById(int id);
}
