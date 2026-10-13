using WorldCupPlanner.Domain;
using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WorldCupPlanner.DataAccess.Repositories;

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
        using AppDbContext context = _contextFactory.CreateDbContext();
        User? existing = context.Users
                                .Include(u => u.Roles)
                                .FirstOrDefault(u => u.idUser == user.idUser);
        if (existing is null) return;                                   // no-op: igual que buscar y no encontrar

        context.Entry(existing).CurrentValues.SetValues(user);          // copia los escalares por nombre de propiedad

        // sincroniza la coleccion de roles a mano (evita el doble tracking de Update sobre grafos)
        List<UserRole> rolesToRemove = existing.Roles.Where(r => !user.HasRole(r.Role)).ToList();
        foreach (UserRole role in rolesToRemove)
        {
            existing.Roles.Remove(role);
            context.Remove(role);
        }

        List<UserRole> rolesToAdd = user.Roles.Where(r => !existing.Roles.Any(e => e.Role == r.Role)).ToList();
        foreach (UserRole role in rolesToAdd)
        {
            existing.Roles.Add(new UserRole { UserId = existing.idUser, User = existing, Role = role.Role });
        }

        context.SaveChanges();                                          // autosave
    }
}
