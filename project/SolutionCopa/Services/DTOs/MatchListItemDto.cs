using ProjectCopa;

namespace Services.DTOs;

// Proyeccion plana de un partido para el listado de la UI: strings ya resueltos
// (equipos, estadio, grupo) para que la pagina no navegue entidades del dominio.
// GroupName es null para partidos de eliminacion; Phase es null para partidos de grupos.
public sealed class MatchListItemDto
{
    public int MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public string? HomeTeamName { get; set; }
    public string? AwayTeamName { get; set; }
    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }
    public string? StadiumName { get; set; }
    public string? GroupName { get; set; }
    public Phase? Phase { get; set; }
    public bool WasSimulated { get; set; }
}
