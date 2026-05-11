using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IGroupRepository
{
    void Add(Group group);
    void Update(Group group);
    void Delete(Group group);
    Group? GetById(int groupId);
    Group? GetByName(string name);
    bool ExistsByName(string name);
    IReadOnlyCollection<Group> GetAll();
}
