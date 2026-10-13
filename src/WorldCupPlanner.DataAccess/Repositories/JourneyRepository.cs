using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

public class JourneyRepository : IJourneyRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public JourneyRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Journey journey)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Journeys.Add(journey);
        context.SaveChanges();
    }

    public void Update(Journey journey)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        if (context.Journeys.Any(stored => stored.Number == journey.Number))
        {
            return;                             // Journey solo tiene Number (PK): nada que actualizar
        }

        context.Journeys.Add(journey);
        context.SaveChanges();
    }

    public void Delete(Journey journey)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        Journey? existing = context.Journeys.FirstOrDefault(stored => stored.Number == journey.Number);
        if (existing is not null)
        {
            context.Journeys.Remove(existing);
            context.SaveChanges();
        }
    }

    public Journey? GetByNumber(int number)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Journeys.AsNoTracking().FirstOrDefault(journey => journey.Number == number);
    }

    public bool ExistsByNumber(int number)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Journeys.AsNoTracking().Any(journey => journey.Number == number);
    }

    public IReadOnlyCollection<Journey> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.Journeys.AsNoTracking().ToList();
    }

    public void Clear()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Journeys.RemoveRange(context.Journeys.ToList());
        context.SaveChanges();
    }
}
