using ProjectCopa.Models;

namespace ProjectCopa;

public sealed class GroupQualifier
{
    private const int TeamsPerGroup = 4;
    private const int FirstPlaceIndex = 0;
    private const int SecondPlaceIndex = 1;
    private const int ThirdPlaceIndex = 2;
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;
    private const int TopFirstsCount = 8;
    private const int RemainingFirstsCount = 4;
    private const int TopSecondsCount = 8;
    private const int WorstSecondsCount = 4;
    private const int BestThirdsCount = 8;
    private const string InvalidGroupSizeMessage = "Each group must contain exactly 4 teams.";

    public QualifiedTeams ClassifyTeams(List<Group> groups, List<Match> playedMatches, int seed)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(playedMatches);

        List<Group> orderedGroups = groups
            .OrderBy(group => group.Name, StringComparer.Ordinal)
            .ToList();

        List<TeamStanding> firstPlaceStandings = new List<TeamStanding>();
        List<TeamStanding> secondPlaceStandings = new List<TeamStanding>();
        List<TeamStanding> thirdPlaceStandings = new List<TeamStanding>();
        Dictionary<int, string> groupByTeamId = new Dictionary<int, string>();

        int groupIndex = 0;
        foreach (Group group in orderedGroups)
        {
            foreach (Team team in group.Teams)
            {
                groupByTeamId[team.Id] = group.Name;
            }

            List<TeamStanding> standings = BuildStandingsForGroup(group, playedMatches, seed, groupIndex);

            if (standings.Count != TeamsPerGroup)
            {
                throw new InvalidOperationException(InvalidGroupSizeMessage);
            }

            List<TeamStanding> orderedStandings = OrderStandings(standings).ToList();
            firstPlaceStandings.Add(orderedStandings[FirstPlaceIndex]);
            secondPlaceStandings.Add(orderedStandings[SecondPlaceIndex]);
            thirdPlaceStandings.Add(orderedStandings[ThirdPlaceIndex]);

            groupIndex++;
        }

        List<TeamStanding> orderedFirsts = OrderStandings(firstPlaceStandings).ToList();
        List<TeamStanding> orderedSeconds = OrderStandings(secondPlaceStandings).ToList();
        List<TeamStanding> orderedThirds = OrderStandings(thirdPlaceStandings).ToList();

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

    private static List<TeamStanding> BuildStandingsForGroup(
        Group group,
        List<Match> playedMatches,
        int seed,
        int groupIndex)
    {
        Dictionary<int, TeamStanding> standings = new Dictionary<int, TeamStanding>();
        foreach (Team team in group.Teams)
        {
            standings[team.Id] = new TeamStanding(team, GetTieBreaker(seed, groupIndex, team.Id));
        }

        List<MatchGroup> groupMatches = playedMatches
            .OfType<MatchGroup>()
            .Where(match => match.Group != null && match.Group.Name == group.Name)
            .ToList();

        foreach (MatchGroup match in groupMatches)
        {
            Team? homeTeam = match.HomeTeam;
            Team? awayTeam = match.AwayTeam;

            if (homeTeam == null || awayTeam == null)
            {
                continue;
            }

            TeamStanding homeStanding = standings[homeTeam.Id];
            TeamStanding awayStanding = standings[awayTeam.Id];

            homeStanding.GoalsFor += match.HomeGoals;
            homeStanding.GoalsAgainst += match.AwayGoals;
            awayStanding.GoalsFor += match.AwayGoals;
            awayStanding.GoalsAgainst += match.HomeGoals;

            if (match.HomeGoals > match.AwayGoals)
            {
                homeStanding.Points += PointsForWin;
            }
            else if (match.HomeGoals < match.AwayGoals)
            {
                awayStanding.Points += PointsForWin;
            }
            else
            {
                homeStanding.Points += PointsForDraw;
                awayStanding.Points += PointsForDraw;
            }
        }

        return standings.Values.ToList();
    }

    private static IOrderedEnumerable<TeamStanding> OrderStandings(IEnumerable<TeamStanding> standings)
    {
        return standings
            .OrderByDescending(standing => standing.Points)
            .ThenByDescending(standing => standing.GoalDifference)
            .ThenByDescending(standing => standing.GoalsFor)
            .ThenByDescending(standing => standing.TieBreaker)
            .ThenBy(standing => standing.Team.Id);
    }

    private static List<Team> TakeTeams(List<TeamStanding> standings, int startIndex, int count)
    {
        return standings
            .Skip(startIndex)
            .Take(count)
            .Select(standing => standing.Team)
            .ToList();
    }

    private static int GetTieBreaker(int seed, int groupIndex, int teamId)
    {
        int mixedSeed = unchecked((seed * 397) ^ (groupIndex * 31) ^ teamId);
        Random random = new Random(mixedSeed);
        return random.Next();
    }

    private sealed class TeamStanding
    {
        public TeamStanding(Team team, int tieBreaker)
        {
            Team = team;
            TieBreaker = tieBreaker;
        }

        public Team Team { get; }

        public int Points { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int GoalDifference => GoalsFor - GoalsAgainst;

        public int TieBreaker { get; }
    }
}
