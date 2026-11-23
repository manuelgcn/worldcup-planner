using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class TournamentStatsService(IMatchRepository matchRepository) : ITournamentStatsService
{
    public TournamentStatsDto GetStats()
    {
        return new TournamentStatsDto();
    }
}
