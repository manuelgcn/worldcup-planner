using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IUserRepository
{
    void Add(User user);
    IReadOnlyCollection<User> GetAll();
    User? GetById(int id);
    User? GetByEmail(string email);
    bool ExistsByEmail(string email);
    void Remove(User user);
    void Update(User user);
}
