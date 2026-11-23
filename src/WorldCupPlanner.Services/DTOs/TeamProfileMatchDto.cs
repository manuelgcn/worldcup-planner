using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

// Un partido visto desde el equipo de la ficha: los goles son a favor y en contra, no local y
// visitante. GroupName es null en eliminatorias y Phase en fase de grupos; los penales son null
// si el partido no se definio por penales.
public sealed record TeamProfileMatchDto(
    int MatchId,
    DateTime MatchDate,
    string? GroupName,
    Phase? Phase,
    int OpponentId,
    string OpponentName,
    string? StadiumName,
    int GoalsFor,
    int GoalsAgainst,
    int? PenaltiesFor,
    int? PenaltiesAgainst,
    bool HasResult);
