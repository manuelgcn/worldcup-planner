using ProjectCopa;

namespace Services.DTOs;

// Single DTO used as both inbound (create/update) and outbound (read) model.
public sealed class MatchDto
{
    public int MatchId { get; init; }

    public DateTime? MatchDate { get; init; }
    public int? HomeTeamId { get; init; }
    public int? AwayTeamId { get; init; }
    public int? StadiumId { get; init; }

    public int HomeGoals { get; init; }
    public int AwayGoals { get; init; }

    public bool IsElimination { get; init; }
    public string? MatchCode { get; init; }
    public Phase? Phase { get; init; }
    public int? MatchHomeId { get; init; }
    public int? MatchAwayId { get; init; }

    public int? ChampionshipConfigurationId { get; init; }
}
