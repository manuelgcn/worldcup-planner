using ProjectCopa;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public User? GetByEmail(string email)
    {
        return context.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.email == email);
    }

    public IReadOnlyCollection<User> GetAll()
    {
        return context.Users
            .Include(u => u.Roles)
            .ToList();
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