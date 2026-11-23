namespace WorldCupPlanner.Services.DTOs;

public sealed record StadiumStatsDto(
    string StadiumName,
    int MatchesPlayed,
    int Goals);
