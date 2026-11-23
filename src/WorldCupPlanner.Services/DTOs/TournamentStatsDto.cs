namespace WorldCupPlanner.Services.DTOs;

// Read model plano de las estadisticas del torneo: todo sale de los partidos con resultado,
// ya contado y ordenado para que la pagina no navegue el grafo de dominio.
// Un partido definido por penales cuenta como empate y, ademas, en PenaltyShootouts.
// BiggestWin es null si ningun partido tuvo ganador en los goles; HighestScoringMatch, si no hubo goles.
public sealed class TournamentStatsDto
{
    public int MatchesPlayed { get; set; }
    public int Goals { get; set; }
    public double GoalsPerMatch { get; set; }
    public int Draws { get; set; }
    public int PenaltyShootouts { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public MatchHighlightDto? BiggestWin { get; set; }
    public MatchHighlightDto? HighestScoringMatch { get; set; }
    public IReadOnlyList<TeamGoalsDto> AttackLeaders { get; set; } = Array.Empty<TeamGoalsDto>();
    public IReadOnlyList<TeamGoalsDto> DefenceLeaders { get; set; } = Array.Empty<TeamGoalsDto>();
    public IReadOnlyList<ConfederationRecordDto> Confederations { get; set; } = Array.Empty<ConfederationRecordDto>();
    public IReadOnlyList<StadiumStatsDto> Stadiums { get; set; } = Array.Empty<StadiumStatsDto>();
}
