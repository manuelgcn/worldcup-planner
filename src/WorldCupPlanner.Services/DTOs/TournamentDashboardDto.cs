namespace WorldCupPlanner.Services.DTOs;

// Read model plano del estado del torneo para la home: conteos y flags ya resueltos
// para que la pagina no consulte repositorios ni navegue el grafo de dominio.
// ChampionName es null mientras la final no este simulada.
public sealed class TournamentDashboardDto
{
    public int TeamCount { get; set; }
    public int StadiumCount { get; set; }
    public int PlayedMatches { get; set; }
    public int TotalMatches { get; set; }
    public bool IsFixtureGenerated { get; set; }
    public bool IsKnockoutGenerated { get; set; }
    public string? ChampionName { get; set; }
}
