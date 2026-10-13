using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IGroupStandingsService
{
    List<GroupStandingDto> GetStandings(string groupName);
    List<GroupStandingDto> GetStandingsForAllGroups();
    bool IsGroupStageComplete();
    int PendingGroupStageMatchCount();
}
