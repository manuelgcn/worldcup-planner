using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

// Repositorio de Team sobre EF Core. Usa IDbContextFactory para crear un contexto
// efimero por operacion (patron recomendado para Blazor Server) y persiste cada
// mutacion al instante (autosave). Las lecturas usan AsNoTracking y resuelven los
// filtros/conteos en la base (IQueryable), sin traer toda la tabla a memoria.
public class TeamRepository : ITeamRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TeamRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Team team)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Teams.Add(team);
        context.SaveChanges();
    }

    public void Update(Team team)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Team? existing = context.Teams.FirstOrDefault(t => t.Id == team.Id);
        if (existing is null)
        {
            context.Teams.Add(team);                                // conserva la semantica actual: si no existe, agrega
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(team);  // copia valores por nombre de propiedad
        }
        context.SaveChanges();
    }

    public void Delete(Team team)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Team? existing = context.Teams.FirstOrDefault(t => t.Id == team.Id);
        if (existing is not null)
        {
            context.Teams.Remove(existing);
            context.SaveChanges();
        }
    }

    public Team? GetById(int teamId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Teams.AsNoTracking().FirstOrDefault(t => t.Id == teamId);
    }

    public Team? GetByName(string name)
    {
        // Comparacion insensible a mayusculas y tildes (CLAUDE.md §7). La tabla
        // tiene a lo sumo 48 equipos, por lo que evaluar en memoria es seguro.
        string target = TextNormalizer.Normalize(name);
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Teams.AsNoTracking().AsEnumerable()
            .FirstOrDefault(t => TextNormalizer.Normalize(t.Name) == target);
    }

    public bool ExistsByName(string name)
    {
        string target = TextNormalizer.Normalize(name);
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Teams.AsNoTracking().AsEnumerable()
            .Any(t => TextNormalizer.Normalize(t.Name) == target);
    }

    public IReadOnlyCollection<Team> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Teams.AsNoTracking().ToList();
    }

    public int CountByConfederation(Confederation confederation)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Teams.AsNoTracking().Count(t => t.Confederation == confederation);
    }
}
