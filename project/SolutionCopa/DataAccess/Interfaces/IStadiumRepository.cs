using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IStadiumRepository
{
    void Add(Stadium stadium);
    void Update(Stadium stadium);
    void Delete(Stadium stadium);
    Stadium? GetById(int stadiumId);
    Stadium? GetByName(string name);
    bool ExistsByName(string name);
    IReadOnlyCollection<Stadium> GetAll();
}