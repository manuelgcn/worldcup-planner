using WorldCupPlanner.Domain.Models;

namespace WorldCupPlanner.Domain;

public sealed class GroupQualifier
{
    private const int TeamsPerGroup = 4;
    private const int FirstPlaceIndex = 0;
    private const int SecondPlaceIndex = 1;
    private const int ThirdPlaceIndex = 2;
    private const int TopFirstsCount = 8;
    private const int RemainingFirstsCount = 4;
    private const int TopSecondsCount = 8;
    private const int WorstSecondsCount = 4;
    private const int BestThirdsCount = 8;
    private const string InvalidGroupSizeMessage = "Cada grupo debe contener exactamente 4 equipos.";

    public QualifiedTeams ClassifyTeams(List<Group> groups, List<Match> playedMatches, int seed)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(playedMatches);

        List<Group> orderedGroups = groups
            .OrderBy(group => group.Name, StringComparer.Ordinal)
            .ToList();

        List<GroupStanding> firstPlaceStandings = new List<GroupStanding>();
        List<GroupStanding> secondPlaceStandings = new List<GroupStanding>();
        List<GroupStanding> thirdPlaceStandings = new List<GroupStanding>();
        Dictionary<int, string> groupByTeamId = new Dictionary<int, string>();

        int groupIndex = 0;
        foreach (Group group in orderedGroups)
        {
            foreach (Team team in group.Teams)
            {
                groupByTeamId[team.Id] = group.Name;
            }

            IEnumerable<Match> groupMatches = playedMatches
                .OfType<MatchGroup>()
                .Where(match => match.Group != null && match.Group.Name == group.Name);
            List<GroupStanding> orderedStandings = RankGroup(group, groupMatches, seed, groupIndex);

            if (orderedStandings.Count != TeamsPerGroup)
            {
                throw new InvalidOperationException(InvalidGroupSizeMessage);
            }

            firstPlaceStandings.Add(orderedStandings[FirstPlaceIndex]);
            secondPlaceStandings.Add(orderedStandings[SecondPlaceIndex]);
            thirdPlaceStandings.Add(orderedStandings[ThirdPlaceIndex]);

            groupIndex++;
        }

        List<GroupStanding> orderedFirsts = OrderStandings(firstPlaceStandings).ToList();
        List<GroupStanding> orderedSeconds = OrderStandings(secondPlaceStandings).ToList();
        List<GroupStanding> orderedThirds = OrderStandings(thirdPlaceStandings).ToList();

        return new QualifiedTeams
        {
            TopEightFirsts = TakeTeams(orderedFirsts, 0, TopFirstsCount),
            RemainingFourFirsts = TakeTeams(orderedFirsts, TopFirstsCount, RemainingFirstsCount),
            TopEightSeconds = TakeTeams(orderedSeconds, 0, TopSecondsCount),
            WorstFourSeconds = TakeTeams(orderedSeconds, TopSecondsCount, WorstSecondsCount),
            BestEightThirds = TakeTeams(orderedThirds, 0, BestThirdsCount),
            GroupByTeamId = groupByTeamId
        };
    }

    // Tabla de un grupo ya ordenada. La usan la clasificacion y la tabla visible
    // (GroupStandingsService), asi las dos no pueden calcular ni ordenar distinto.
    // 'matches' son los partidos de ese grupo; los que no tienen resultado no cuentan.
    public static List<GroupStanding> RankGroup(Group group, IEnumerable<Match> matches, int seed, int groupIndex)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(matches);

        Dictionary<int, GroupStanding> standings = group.Teams.ToDictionary(
            team => team.Id,
            team => new GroupStanding(team, GroupTieBreaker.Compute(seed, groupIndex, team.Id)));

        foreach (Match match in matches.Where(match => match.HasResult))
        {
            if (match.HomeTeam == null || match.AwayTeam == null
                || !standings.TryGetValue(match.HomeTeam.Id, out GroupStanding? home)
                || !standings.TryGetValue(match.AwayTeam.Id, out GroupStanding? away))
            {
                continue;
            }

            home.RegisterResult(match.HomeGoals, match.AwayGoals);
            away.RegisterResult(match.AwayGoals, match.HomeGoals);
        }

        return OrderStandings(standings.Values).ToList();
    }

    private static IOrderedEnumerable<GroupStanding> OrderStandings(IEnumerable<GroupStanding> standings)
    {
        return standings
            .OrderByDescending(standing => standing.Points)
            .ThenByDescending(standing => standing.GoalDifference)
            .ThenByDescending(standing => standing.GoalsFor)
            .ThenByDescending(standing => standing.TieBreaker)
            .ThenBy(standing => standing.Team.Id);
    }

    private static List<Team> TakeTeams(List<GroupStanding> standings, int startIndex, int count)
    {
        return standings
            .Skip(startIndex)
            .Take(count)
            .Select(standing => standing.Team)
            .ToList();
    }
}
