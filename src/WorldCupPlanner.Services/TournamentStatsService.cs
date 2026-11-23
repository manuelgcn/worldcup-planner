using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Controlador GRASP del caso de uso "ver estadisticas del torneo": una UNICA lectura de la
// jerarquia TPH (IMatchRepository.GetAll ya trae equipos, estadio e incidencias) y todo el
// resto se calcula en memoria sobre los partidos con resultado.
public class TournamentStatsService(IMatchRepository matchRepository) : ITournamentStatsService
{
    private const int LeaderCount = 5;
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;

    public TournamentStatsDto GetStats()
    {
        List<Match> played = matchRepository.GetAll().Where(IsPlayed).ToList();
        int goals = played.Sum(TotalGoals);
        List<TeamSide> sides = played.SelectMany(Sides).ToList();
        List<TeamGoalsDto> teams = BuildTeamRows(sides);

        return new TournamentStatsDto
        {
            MatchesPlayed = played.Count,
            Goals = goals,
            GoalsPerMatch = played.Count == 0 ? 0 : (double)goals / played.Count,
            Draws = played.Count(match => match.IsDraw()),
            PenaltyShootouts = played.OfType<MatchElimination>().Count(match => match.WonByPenalties),
            YellowCards = CountIncidents<YellowCardIncident>(played),
            RedCards = CountIncidents<RedCardIncident>(played),
            BiggestWin = FirstHighlight(played
                .Where(match => !match.IsDraw())
                .OrderByDescending(match => Math.Abs(match.GetGoalDifferenceForHome()))
                .ThenByDescending(TotalGoals)),
            HighestScoringMatch = FirstHighlight(played
                .Where(match => TotalGoals(match) > 0)
                .OrderByDescending(TotalGoals)),
            AttackLeaders = TopByName(teams.OrderByDescending(team => team.GoalsFor)),
            DefenceLeaders = TopByName(teams
                .OrderBy(team => team.GoalsAgainst)
                .ThenByDescending(team => team.MatchesPlayed)),
            Confederations = BuildConfederationRecords(sides),
            Stadiums = BuildStadiumRows(played)
        };
    }

    // Lo que un partido significo para uno de sus dos equipos.
    private sealed record TeamSide(Team Team, int GoalsFor, int GoalsAgainst);

    private static IEnumerable<TeamSide> Sides(Match match)
    {
        yield return new TeamSide(match.HomeTeam!, match.HomeGoals, match.AwayGoals);
        yield return new TeamSide(match.AwayTeam!, match.AwayGoals, match.HomeGoals);
    }

    // Agrupa por id y no por instancia: el mismo equipo puede llegar en objetos distintos.
    private static List<TeamGoalsDto> BuildTeamRows(IEnumerable<TeamSide> sides)
    {
        return sides
            .GroupBy(side => side.Team.Id)
            .Select(group => new TeamGoalsDto(
                group.Key,
                group.First().Team.Name,
                group.First().Team.Confederation,
                group.Count(),
                group.Sum(side => side.GoalsFor),
                group.Sum(side => side.GoalsAgainst)))
            .ToList();
    }

    // Los empates se ordenan por nombre normalizado (sin tildes ni mayusculas) y luego por id.
    private static List<TeamGoalsDto> TopByName(IOrderedEnumerable<TeamGoalsDto> ranked)
    {
        return ranked
            .ThenBy(team => TextNormalizer.Normalize(team.TeamName), StringComparer.Ordinal)
            .ThenBy(team => team.TeamId)
            .Take(LeaderCount)
            .ToList();
    }

    // Solo miran los goles: un partido definido por penales es empate para los dos equipos.
    private static List<ConfederationRecordDto> BuildConfederationRecords(IEnumerable<TeamSide> sides)
    {
        return sides
            .GroupBy(side => side.Team.Confederation)
            .Select(group =>
            {
                int wins = group.Count(side => side.GoalsFor > side.GoalsAgainst);
                int draws = group.Count(side => side.GoalsFor == side.GoalsAgainst);
                return new ConfederationRecordDto(
                    group.Key,
                    group.Select(side => side.Team.Id).Distinct().Count(),
                    wins,
                    draws,
                    group.Count() - wins - draws,
                    (double)(wins * PointsForWin + draws * PointsForDraw) / group.Count());
            })
            .OrderByDescending(record => record.PointsPerMatch)
            .ThenBy(record => record.Confederation.ToString(), StringComparer.Ordinal)
            .ToList();
    }

    private static List<StadiumStatsDto> BuildStadiumRows(IEnumerable<Match> played)
    {
        return played
            .Where(match => match.Stadium is not null)
            .GroupBy(match => match.Stadium!.Id)
            .Select(group => new StadiumStatsDto(group.First().Stadium!.Name, group.Count(), group.Sum(TotalGoals)))
            .OrderByDescending(stadium => stadium.Goals)
            .ThenBy(stadium => TextNormalizer.Normalize(stadium.StadiumName), StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsPlayed(Match match) =>
        match.HasResult && match.HomeTeam is not null && match.AwayTeam is not null;

    private static int TotalGoals(Match match) => match.HomeGoals + match.AwayGoals;

    private static int CountIncidents<TIncident>(IEnumerable<Match> matches) where TIncident : MatchIncident =>
        matches.Sum(match => match.Incidents.OfType<TIncident>().Count());

    // Los empates del criterio se resuelven por fecha y luego por id: el destacado no depende
    // del orden en que el repositorio devuelva los partidos.
    private static MatchHighlightDto? FirstHighlight(IOrderedEnumerable<Match> ranked)
    {
        Match? match = ranked
            .ThenBy(candidate => candidate.MatchDate)
            .ThenBy(candidate => candidate.MatchId)
            .FirstOrDefault();

        return match is null
            ? null
            : new MatchHighlightDto(match.HomeTeam!.Name, match.AwayTeam!.Name, match.HomeGoals, match.AwayGoals);
    }
}
