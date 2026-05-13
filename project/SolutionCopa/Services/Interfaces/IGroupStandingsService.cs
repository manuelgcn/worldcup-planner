using Services.DTOs;

namespace Services.Interfaces;

public interface IGroupStandingsService
{
    List<GroupStandingDto> GetStandings(string groupName);
    bool IsGroupStageComplete();
    int PendingGroupStageMatchCount();
}
