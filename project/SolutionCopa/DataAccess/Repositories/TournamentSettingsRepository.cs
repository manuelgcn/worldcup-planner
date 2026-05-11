using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class TournamentSettingsRepository : ITournamentSettingsRepository
{
    private readonly List<TournamentSettings> _settings = new();

    public void Add(TournamentSettings settings)
    {
        _settings.Add(settings);
    }

    public void Update(TournamentSettings settings)
    {
        int index = _settings.FindIndex(existingSettings => existingSettings.Id == settings.Id);

        if (index >= 0)
        {
            _settings[index] = settings;
            return;
        }

        _settings.Add(settings);
    }

    public void Delete(TournamentSettings settings)
    {
        _settings.RemoveAll(existingSettings => existingSettings.Id == settings.Id);
    }

    public TournamentSettings? GetById(int tournamentSettingsId)
    {
        return _settings.FirstOrDefault(settings => settings.Id == tournamentSettingsId);
    }

    public IReadOnlyCollection<TournamentSettings> GetAll()
    {
        return _settings.AsReadOnly();
    }
}
