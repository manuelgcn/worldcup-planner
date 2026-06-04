using ProjectCopa;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public UserRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(User user)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Users.Add(user);
        context.SaveChanges();
    }

    public User? GetById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Users.AsNoTracking().Include(u => u.Roles).FirstOrDefault(u => u.idUser == id);
    }

    public User? GetByEmail(string email)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Users.AsNoTracking().Include(u => u.Roles).FirstOrDefault(u => u.email == email);
    }

    public bool ExistsByEmail(string email)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Users.AsNoTracking().Any(u => u.email == email);
    }

    public IReadOnlyCollection<User> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Users.AsNoTracking().Include(u => u.Roles).ToList();
    }

    public void Remove(User user)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        User? existing = context.Users.Include(u => u.Roles).FirstOrDefault(u => u.idUser == user.idUser);
        if (existing is null) return;
        context.Users.Remove(existing);
        context.SaveChanges();
    }

    public void Update(User user)
    {
        // TODO(GREEN): version ingenua. Update() sobre el grafo no sincroniza la
        // coleccion de roles (no entera de los roles quitados), por lo que los tests
        // de sincronizacion de roles y de no-op para usuarios inexistentes fallan.
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        context.SaveChanges();
    }
}
