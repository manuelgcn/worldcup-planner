using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public GroupRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Group group)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        AttachExistingTeams(context, group);   // los equipos ya existen (TeamRepository): attach, no insert
        context.Add(group);                     // inserta el grupo y fija el FK GroupName de sus equipos
        context.SaveChanges();
    }

    public void Update(Group group)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        if (context.Groups.Any(stored => stored.Name == group.Name))
        {
            return;                             // Group no tiene escalares mutables (Name es la PK)
        }

        AttachExistingTeams(context, group);
        context.Add(group);
        context.SaveChanges();
    }

    public void Delete(Group group)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Group? existing = context.Groups.Include(stored => stored.Teams).FirstOrDefault(stored => stored.Name == group.Name);
        if (existing is not null)
        {
            context.Groups.Remove(existing);    // SetNull: los equipos sobreviven, se libera su GroupName
            context.SaveChanges();
        }
    }

    public Group? GetByName(string name)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Groups
            .AsNoTrackingWithIdentityResolution()
            .Include(group => group.Teams)
            .FirstOrDefault(group => group.Name == name);
    }

    public bool ExistsByName(string name)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Groups.AsNoTracking().Any(group => group.Name == name);
    }

    public IReadOnlyCollection<Group> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Groups
            .AsNoTrackingWithIdentityResolution()
            .Include(group => group.Teams)
            .ToList();
    }

    public void Clear()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        // Include(Teams) para que ClientSetNull/SetNull nullee el GroupName de los equipos trackeados.
        List<Group> groups = context.Groups.Include(group => group.Teams).ToList();
        context.Groups.RemoveRange(groups);
        context.SaveChanges();
    }

    // Marca como Unchanged (sin recorrer el grafo) los equipos ya persistidos del grupo, para que el
    // Add inserte solo el grupo y EF actualice el FK GroupName de cada equipo en lugar de reinsertarlos.
    private static void AttachExistingTeams(AppDbContext context, Group group)
    {
        foreach (Team team in group.Teams)
        {
            if (context.Entry(team).State == EntityState.Detached)
            {
                context.Entry(team).State = EntityState.Unchanged;
            }
        }
    }
}
