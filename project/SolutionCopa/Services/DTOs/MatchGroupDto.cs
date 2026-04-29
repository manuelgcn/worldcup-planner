using ProjectCopa.Interfaces;

namespace Services.DTOs;

public sealed class MatchGroupDto
{
    public MatchGroupDto(int matchId, DateTime? matchDate, ITeam homeTeamId, ITeam awayTeamId, IStadium stadiumId, int homeGoals, int awayGoals, IGroup group, IJourney journey)
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
    public ITeam HomeTeamId { get; init; }
    public ITeam AwayTeamId { get; init; }
    public IStadium StadiumId { get; init; }

    public int HomeGoals { get; init; }
    public int AwayGoals { get; init; }
    
    
    public IGroup Group { get; init; }
    public IJourney Journey { get; init; }
}