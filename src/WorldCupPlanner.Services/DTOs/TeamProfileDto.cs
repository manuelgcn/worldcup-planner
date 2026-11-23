using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

// Read model plano de la ficha de un equipo, para que la pagina no navegue el grafo de dominio.
// GroupName y GroupPosition son null mientras el equipo no tenga partidos de grupo.
// El record cuenta todos los partidos con resultado, eliminatorias incluidas; uno definido
// por penales cuenta como empate.
public sealed class TeamProfileDto
{
    public int TeamId { get; set; }
    public string Name { get; set; } = "";
    public string? FlagBase64 { get; set; }
    public Confederation Confederation { get; set; }
    public int FifaRanking { get; set; }
    public bool IsFictitious { get; set; }
    public string? GroupName { get; set; }
    public int? GroupPosition { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }
}
