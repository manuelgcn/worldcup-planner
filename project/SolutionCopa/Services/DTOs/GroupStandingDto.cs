using ProjectCopa;

namespace Services.DTOs;

public sealed record GroupStandingDto(
    Team Team,
    int GamesPlayed,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
