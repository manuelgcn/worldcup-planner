using ProjectCopa;
using DataAccess.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public class MockUserRepository : IUserRepository
{
    private readonly List<User> users = new();

    public void Add(User user)
    {
        users.Add(user);
    }

    public IReadOnlyCollection<User> GetAll()
    {
        return users.ToList();
    }

    public User? GetById(int id)
    {
        return users.FirstOrDefault(u => u.idUser == id);
    }

    public User? GetByEmail(string email)
    {
        return users.FirstOrDefault(u => u.email == email);
    }

    public bool ExistsByEmail(string email)
    {
        return users.Any(u => u.email == email);
    }

    public void Remove(User user)
    {
        users.Remove(user);
    }

    public void Update(User user)
    {
    }
}
