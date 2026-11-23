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
    public TournamentStatsDto GetStats()
    {
        List<Match> played = matchRepository.GetAll().Where(IsPlayed).ToList();
        int goals = played.Sum(TotalGoals);

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
                .OrderByDescending(TotalGoals))
        };
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
