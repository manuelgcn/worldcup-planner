using Services.DTOs;

namespace Services.Interfaces;

public interface IGroupStandingsService
{
    List<GroupStandingDto> GetStandings(string groupName);
    List<GroupStandingDto> GetStandingsForAllGroups();
    bool IsGroupStageComplete();
    int PendingGroupStageMatchCount();
}
