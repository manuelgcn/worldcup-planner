using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

public sealed record TeamGoalsDto(
    int TeamId,
    string TeamName,
    Confederation Confederation,
    int MatchesPlayed,
    int GoalsFor,
    int GoalsAgainst);
