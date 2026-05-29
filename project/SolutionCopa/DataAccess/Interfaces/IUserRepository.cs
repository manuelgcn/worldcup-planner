using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IUserRepository
{
    void Add(User user);
    IReadOnlyCollection<User> GetAll();
    User? GetByEmail(string email);
    void Remove(User user);
    void Update(User user);
}