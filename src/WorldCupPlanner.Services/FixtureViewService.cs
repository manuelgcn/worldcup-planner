using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Controlador GRASP del caso de uso "ver estado del fixture": coordina los repositorios de
// settings, equipos, grupos y partidos para armar el read model de la pagina /fixture.
// Consulta SOLO IMatchRepository (que ya representa toda la jerarquia TPH) para no duplicar
// partidos eliminatorios, igual que TournamentDashboardService.
public class FixtureViewService(
    IMatchRepository matchRepository,
    IGroupRepository groupRepository,
    ITeamRepository teamRepository,
    ITournamentSettingsRepository tournamentSettingsRepository) : IFixtureViewService
{
    public FixtureSnapshotDto GetSnapshot()
    {
        List<Match> matches = matchRepository.GetAll();
        List<DateTime> matchDates = matches.Select(match => match.MatchDate).ToList();

        return new FixtureSnapshotDto
        {
            Settings = tournamentSettingsRepository.GetAll().FirstOrDefault(),
            TeamCount = teamRepository.GetAll().Count,
            Groups = groupRepository.GetAll().OrderBy(group => group.Name).ToList(),
            GroupMatches = matches.OfType<MatchGroup>().ToList(),
            EliminationMatches = matches.OfType<MatchElimination>().ToList(),
            EarliestMatchDate = matchDates.Count > 0 ? matchDates.Min() : null,
            LatestMatchDate = matchDates.Count > 0 ? matchDates.Max() : null
        };
    }
}
