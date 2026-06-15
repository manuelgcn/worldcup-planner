using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

// Doble en memoria de ITournamentSettingsRepository para los tests de servicios, que no
// dependen de EF Core (mismo rol que MockStadiumRepository / MockTeamRepository). Reemplaza
// el uso directo del TournamentSettingsRepository real, ahora respaldado por IDbContextFactory.
public sealed class MockTournamentSettingsRepository : ITournamentSettingsRepository
{
    private readonly Dictionary<int, TournamentSettings> _settings = new();

    public void Add(TournamentSettings settings)
    {
        _settings[settings.Id] = settings;
    }

    public void Update(TournamentSettings settings)
    {
        _settings[settings.Id] = settings;
    }

    public void Delete(TournamentSettings settings)
    {
        _settings.Remove(settings.Id);
    }

    public TournamentSettings? GetById(int tournamentSettingsId)
    {
        return _settings.GetValueOrDefault(tournamentSettingsId);
    }

    public IReadOnlyCollection<TournamentSettings> GetAll()
    {
        return _settings.Values.ToList();
    }
}
