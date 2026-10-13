using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

// Read model del estado del fixture para la pagina /fixture: settings del torneo, conteo de
// equipos, grupos y partidos ya consultados, y el rango de fechas para los valores por defecto
// de exportacion. Lo arma FixtureViewService consultando SOLO IMatchRepository para los
// partidos (la jerarquia TPH ya trae grupos y eliminatorias), evitando duplicar eliminatorias.
//
// Expone entidades de dominio (Group, MatchGroup, MatchElimination) porque los componentes
// GroupCard / GroupDetail / KnockoutBracket todavia las consumen; la proyeccion completa del
// bracket a DTOs planos queda como corte posterior.
public sealed class FixtureSnapshotDto
{
    public TournamentSettings? Settings { get; init; }
    public int TeamCount { get; init; }
    public IReadOnlyList<Group> Groups { get; init; } = Array.Empty<Group>();
    public IReadOnlyList<MatchGroup> GroupMatches { get; init; } = Array.Empty<MatchGroup>();
    public IReadOnlyList<MatchElimination> EliminationMatches { get; init; } = Array.Empty<MatchElimination>();
    public DateTime? EarliestMatchDate { get; init; }
    public DateTime? LatestMatchDate { get; init; }
}
