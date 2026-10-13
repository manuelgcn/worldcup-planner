using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

// Repositorio de TournamentSettings sobre EF Core (mismo patron que TeamRepository).
// La entidad es inmutable (propiedades get-only): SetValues escribe los backing fields.
public class TournamentSettingsRepository : ITournamentSettingsRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TournamentSettingsRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(TournamentSettings settings)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.TournamentSettings.Add(settings);
        context.SaveChanges();
    }

    public void Update(TournamentSettings settings)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        TournamentSettings? existing = context.TournamentSettings.FirstOrDefault(s => s.Id == settings.Id);
        if (existing is null)
        {
            context.TournamentSettings.Add(settings);                  // conserva la semantica actual: si no existe, agrega
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(settings); // copia valores por nombre de propiedad (backing fields)
        }
        context.SaveChanges();
    }

    public void Delete(TournamentSettings settings)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        TournamentSettings? existing = context.TournamentSettings.FirstOrDefault(s => s.Id == settings.Id);
        if (existing is not null)
        {
            context.TournamentSettings.Remove(existing);
            context.SaveChanges();
        }
    }

    public TournamentSettings? GetById(int tournamentSettingsId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.TournamentSettings.AsNoTracking().FirstOrDefault(s => s.Id == tournamentSettingsId);
    }

    public IReadOnlyCollection<TournamentSettings> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.TournamentSettings.AsNoTracking().ToList();
    }
}
