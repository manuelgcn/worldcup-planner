using ProjectCopa;
using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext context;

    public UserRepository(AppDbContext context)
    {
        this.context = context;
    }

    public void Add(User user)
    {
        context.Users.Add(user);
        context.SaveChanges();
    }

    public IReadOnlyCollection<User> GetAll()
    {
        return context.Users.ToList();
    }

    public User? GetByEmail(string email)
    {
        return context.Users.FirstOrDefault(u => u.email == email);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
        context.SaveChanges();
    }

    public void Update(User user)
    {
        context.Users.Update(user);
        context.SaveChanges();
    }
}