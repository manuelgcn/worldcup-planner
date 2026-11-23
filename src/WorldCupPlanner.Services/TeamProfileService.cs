using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class TeamProfileService(
    ITeamRepository teamRepository,
    IMatchRepository matchRepository,
    IGroupStandingsService standingsService) : ITeamProfileService
{
    public TeamProfileDto GetProfile(int teamId)
    {
        return new TeamProfileDto();
    }
}
