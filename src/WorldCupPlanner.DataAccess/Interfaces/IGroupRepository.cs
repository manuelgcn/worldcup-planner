using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IGroupRepository
{
    void Add(Group group);
    void Update(Group group);
    void Delete(Group group);
    Group? GetByName(string name);
    bool ExistsByName(string name);
    IReadOnlyCollection<Group> GetAll();
    // Los repositorios EF lo resuelven con un EXISTS; el default sirve a los dobles de test.
    bool Any() => GetAll().Count > 0;
    void Clear();
}
