using ProjectCopa;

namespace Services.DTOs;

public sealed class MatchGroupDto
{
    public MatchGroupDto(int matchId, DateTime? matchDate, Team homeTeamId, Team awayTeamId, Stadium stadiumId, int homeGoals, int awayGoals, Group group, Journey journey)
    {
        MatchId = matchId;
        MatchDate = matchDate;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        StadiumId = stadiumId;
        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        Group = group;
        Journey = journey;
    }

    public int MatchId { get; init; }

    public DateTime? MatchDate { get; init; }
    public Team HomeTeamId { get; init; }
    public Team AwayTeamId { get; init; }
    public Stadium StadiumId { get; init; }

    public int HomeGoals { get; init; }
    public int AwayGoals { get; init; }
    
    
    public Group Group { get; init; }
    public Journey Journey { get; init; }
}
