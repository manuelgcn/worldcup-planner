using ProjectCopa;

namespace DataAccess.Interfaces;

public interface ITournamentSettingsRepository
{
    TournamentSettings? GetById(int tournamentSettingsId);
}
