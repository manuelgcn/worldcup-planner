using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IJourneyRepository
{
    void Add(Journey journey);
    void Update(Journey journey);
    void Delete(Journey journey);
    Journey? GetByNumber(int number);
    bool ExistsByNumber(int number);
    IReadOnlyCollection<Journey> GetAll();
}
