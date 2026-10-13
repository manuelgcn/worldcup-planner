using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

public sealed class MatchDto
{
    public int MatchId { get; set; }

    public DateTime? MatchDate { get; set; }
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? StadiumId { get; set; }

    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }

    public bool IsElimination { get; set; }
    public string? GroupName { get; set; }
    public int? JourneyNumber { get; set; }
    public string? MatchCode { get; set; }
    public Phase? Phase { get; set; }
    public int? MatchHomeId { get; set; }
    public int? MatchAwayId { get; set; }

    public int? TournamentSettingsId { get; set; }
}
