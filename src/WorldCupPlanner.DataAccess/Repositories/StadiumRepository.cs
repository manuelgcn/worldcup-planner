using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

// Repositorio de Stadium sobre EF Core. Mismo patron que TeamRepository:
// IDbContextFactory para un contexto efimero por operacion (recomendado en Blazor Server),
// autosave por mutacion y lecturas AsNoTracking con los filtros resueltos en la base.
public class StadiumRepository : IStadiumRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public StadiumRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Stadium stadium)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Stadiums.Add(stadium);
        context.SaveChanges();
    }

    public void Update(Stadium stadium)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Stadium? existing = context.Stadiums.FirstOrDefault(s => s.Id == stadium.Id);
        if (existing is null)
        {
            context.Stadiums.Add(stadium);                              // conserva la semantica actual: si no existe, agrega
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(stadium);   // copia valores por nombre de propiedad
        }
        context.SaveChanges();
    }

    public void Delete(Stadium stadium)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Stadium? existing = context.Stadiums.FirstOrDefault(s => s.Id == stadium.Id);
        if (existing is not null)
        {
            context.Stadiums.Remove(existing);
            context.SaveChanges();
        }
    }

    public Stadium? GetById(int stadiumId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Stadiums.AsNoTracking().FirstOrDefault(s => s.Id == stadiumId);
    }

    public Stadium? GetByName(string name)
    {
        // Comparacion insensible a mayusculas y tildes (CLAUDE.md §7); la tabla de
        // estadios es chica, por lo que evaluar en memoria no tiene costo relevante.
        string target = TextNormalizer.Normalize(name);
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Stadiums.AsNoTracking().AsEnumerable()
            .FirstOrDefault(s => TextNormalizer.Normalize(s.Name) == target);
    }

    public bool ExistsByName(string name)
    {
        string target = TextNormalizer.Normalize(name);
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Stadiums.AsNoTracking().AsEnumerable()
            .Any(s => TextNormalizer.Normalize(s.Name) == target);
    }

    public IReadOnlyCollection<Stadium> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Stadiums.AsNoTracking().ToList();
    }
}
