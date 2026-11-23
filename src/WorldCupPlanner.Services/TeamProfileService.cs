using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Controlador GRASP del caso de uso "ver la ficha de un equipo": una lectura del equipo, una de
// los partidos y la tabla de su grupo, que la arma IGroupStandingsService para que el desempate
// viva en un solo lugar.
public class TeamProfileService(
    ITeamRepository teamRepository,
    IMatchRepository matchRepository,
    IGroupStandingsService standingsService) : ITeamProfileService
{
    private const string TeamNotFoundMessage = "No se encontró el equipo.";
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;

    public TeamProfileDto GetProfile(int teamId)
    {
        Team team = teamRepository.GetById(teamId)
            ?? throw new TeamServicesException(TeamNotFoundMessage);

        // IMatchRepository no filtra por equipo: se leen todos (104 como mucho) y se filtra aca.
        // Se compara por id y no por instancia: cada lectura devuelve objetos nuevos.
        List<TeamMatch> matches = matchRepository.GetAll()
            .Where(match => match.HomeTeam is not null && match.AwayTeam is not null)
            .Where(match => match.HomeTeam!.Id == teamId || match.AwayTeam!.Id == teamId)
            .OrderBy(match => match.MatchDate)
            .ThenBy(match => match.MatchId)
            .Select(match => new TeamMatch(match, match.HomeTeam!.Id == teamId))
            .ToList();
        List<TeamMatch> played = matches.Where(match => match.Match.HasResult).ToList();
        // Solo miran los goles: un partido definido por penales es empate.
        int won = played.Count(match => match.GoalsFor > match.GoalsAgainst);
        int drawn = played.Count(match => match.GoalsFor == match.GoalsAgainst);
        string? groupName = matches
            .Select(match => (match.Match as MatchGroup)?.Group?.Name)
            .FirstOrDefault(name => name is not null);

        return new TeamProfileDto
        {
            TeamId = team.Id,
            Name = team.Name,
            FlagBase64 = team.FlagBase64,
            Confederation = team.Confederation,
            FifaRanking = team.FifaRanking,
            IsFictitious = team.IsFictitious,
            GroupName = groupName,
            GroupPosition = GroupPosition(groupName, teamId),
            Played = played.Count,
            Won = won,
            Drawn = drawn,
            Lost = played.Count - won - drawn,
            GoalsFor = played.Sum(match => match.GoalsFor),
            GoalsAgainst = played.Sum(match => match.GoalsAgainst),
            Points = won * PointsForWin + drawn * PointsForDraw
        };
    }

    // Un partido visto desde el equipo de la ficha.
    private sealed record TeamMatch(Match Match, bool IsHome)
    {
        public int GoalsFor => IsHome ? Match.HomeGoals : Match.AwayGoals;
        public int GoalsAgainst => IsHome ? Match.AwayGoals : Match.HomeGoals;
    }

    private int? GroupPosition(string? groupName, int teamId)
    {
        if (groupName is null)
        {
            return null;
        }

        int index = standingsService.GetStandings(groupName).FindIndex(standing => standing.Team.Id == teamId);
        return index < 0 ? null : index + 1;
    }
}
