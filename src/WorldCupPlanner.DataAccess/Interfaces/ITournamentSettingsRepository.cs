using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface ITournamentSettingsRepository
{
    void Add(TournamentSettings settings);
    void Update(TournamentSettings settings);
    void Delete(TournamentSettings settings);
    TournamentSettings? GetById(int tournamentSettingsId);
    IReadOnlyCollection<TournamentSettings> GetAll();
}
