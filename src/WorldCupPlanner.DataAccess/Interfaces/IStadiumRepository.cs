using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IStadiumRepository
{
    void Add(Stadium stadium);
    void Update(Stadium stadium);
    void Delete(Stadium stadium);
    Stadium? GetById(int stadiumId);
    Stadium? GetByName(string name);
    bool ExistsByName(string name);
    IReadOnlyCollection<Stadium> GetAll();
    // El repositorio EF lo resuelve con un COUNT; el default sirve a los dobles de test.
    int Count() => GetAll().Count;
}