using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Interfaces;

namespace Services;

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
        List<Match> matches = matchRepository.GetAll();

        return new TournamentDashboardDto
        {
            TeamCount = teamRepository.GetAll().Count,
            StadiumCount = stadiumRepository.GetAll().Count,
            TotalMatches = matches.Count,
            PlayedMatches = matches.Count(IsPlayed),
            IsFixtureGenerated = fixtureOrchestrator.IsFixtureGenerated(),
            IsKnockoutGenerated = fixtureOrchestrator.IsKnockoutGenerated(),
            ChampionName = FindChampionName(matches)
        };
    }

    private static bool IsPlayed(Match match) =>
        match.WasSimulated || match.HomeGoals + match.AwayGoals > 0;

    private static string? FindChampionName(IEnumerable<Match> matches)
    {
        MatchElimination? final = matches
            .OfType<MatchElimination>()
            .FirstOrDefault(match => match.Phase == Phase.Final);

        if (final is null || !final.WasSimulated)
        {
            return null;
        }

        return final.GetWinner()?.Name;
    }
}
