namespace Services.DTOs;

public sealed class FixtureExportRow
{
    public int MatchId { get; init; }
    public DateTime MatchDate { get; init; }
    public string Stage { get; init; } = string.Empty;
    public string? Group { get; init; }
    public int? Journey { get; init; }
    public string? MatchCode { get; init; }
    public string? HomeTeam { get; init; }
    public string? AwayTeam { get; init; }
    public string? Stadium { get; init; }
    public int HomeGoals { get; init; }
    public int AwayGoals { get; init; }
    public bool WasSimulated { get; init; }
    public int? HomePenaltyGoals { get; init; }
    public int? AwayPenaltyGoals { get; init; }
}
