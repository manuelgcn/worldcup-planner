using ProjectCopa;

namespace Services.DTOs;

public sealed class UpdateMatchRequestDto
{
    public DateTime? MatchDate { get; init; }
    public int? HomeTeamId { get; init; }
    public int? AwayTeamId { get; init; }
    public int? StadiumId { get; init; }

    public int? ChampionshipConfigurationId { get; init; }
    public int? GroupId { get; init; }
    public int? JourneyId { get; init; }
    public Phase? Phase { get; init; }
    public string? MatchCode { get; init; }
    public int? MatchHomeId { get; init; }
    public int? MatchAwayId { get; init; }
}
