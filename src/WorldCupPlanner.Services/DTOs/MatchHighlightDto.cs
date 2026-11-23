namespace WorldCupPlanner.Services.DTOs;

public sealed record MatchHighlightDto(
    string HomeTeamName,
    string AwayTeamName,
    int HomeGoals,
    int AwayGoals);
