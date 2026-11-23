using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Controlador GRASP del caso de uso "ver estado del torneo": coordina los repositorios de
// equipos, estadios y partidos y el orquestador de fixture para armar el read model de la home.
// Consulta SOLO IMatchRepository (que ya representa toda la jerarquia TPH) para no duplicar
// partidos eliminatorios.
public class TournamentDashboardService(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IStadiumRepository stadiumRepository,
    IFixtureOrchestrator fixtureOrchestrator) : ITournamentDashboardService
{
    public TournamentDashboardDto GetOverview()
    {
        return new TournamentDashboardDto
        {
            TeamCount = teamRepository.Count(),
            StadiumCount = stadiumRepository.Count(),
            TotalMatches = matchRepository.Count(),
            PlayedMatches = matchRepository.CountPlayed(),
            IsFixtureGenerated = fixtureOrchestrator.IsFixtureGenerated(),
            IsKnockoutGenerated = fixtureOrchestrator.IsKnockoutGenerated(),
            ChampionName = FindChampionName()
        };
    }

    // Solo se lee la final (consulta filtrada por fase), no toda la tabla de partidos.
    private string? FindChampionName()
    {
        MatchElimination? final = matchRepository.GetByPhase(Phase.Final)
            .OfType<MatchElimination>()
            .FirstOrDefault();

        if (final is null || !final.HasResult)
        {
            return null;
        }

        return final.GetWinner()?.Name;
    }
}
