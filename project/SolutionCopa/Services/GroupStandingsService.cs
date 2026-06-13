using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class GroupStandingsService : IGroupStandingsService
{
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;
    private const int DefaultTieBreakerSeed = 0;
    private const string GroupNotFoundMessage = "Group was not found.";

    private readonly IMatchRepository _matchRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;

    public GroupStandingsService(
        IMatchRepository matchRepository,
        IGroupRepository groupRepository,
        ITournamentSettingsRepository settingsRepository)
    {
        _matchRepository = matchRepository;
        _groupRepository = groupRepository;
        _settingsRepository = settingsRepository;
    }

    public List<GroupStandingDto> GetStandings(string groupName)
    {
        Group group = _groupRepository.GetByName(groupName)
            ?? throw new GroupServicesException(GroupNotFoundMessage);

        Dictionary<int, StandingAccumulator> accumulators = InitializeAccumulators(group);
        AccumulateGroupMatches(group, accumulators);

        IReadOnlyCollection<StandingAccumulator> rows = accumulators.Values.ToList();
        Dictionary<int, int> tieBreakers = BuildTieBreakerMap(rows, ResolveTieBreakerSeed());

        return OrderStandings(rows, tieBreakers)
            .Select(accumulator => accumulator.ToDto())
            .ToList();
    }

    public List<GroupStandingDto> GetStandingsForAllGroups()
    {
        return _groupRepository.GetAll()
            .OrderBy(group => group.Name)
            .SelectMany(group => GetStandings(group.Name))
            .ToList();
    }

    public bool IsGroupStageComplete()
    {
        IReadOnlyCollection<MatchGroup> groupMatches = GetAllGroupMatches();
        if (groupMatches.Count == 0)
        {
            return false;
        }

        return groupMatches.All(IsMatchPlayed);
    }

    public int PendingGroupStageMatchCount()
    {
        return GetAllGroupMatches().Count(match => !IsMatchPlayed(match));
    }

    private static Dictionary<int, StandingAccumulator> InitializeAccumulators(Group group)
    {
        return group.Teams.ToDictionary(team => team.Id, team => new StandingAccumulator(team));
    }

    private void AccumulateGroupMatches(Group group, Dictionary<int, StandingAccumulator> accumulators)
    {
        IEnumerable<MatchGroup> matches = _matchRepository.GetByGroup(group.Name).OfType<MatchGroup>();
        foreach (MatchGroup match in matches)
        {
            AccumulateMatch(match, accumulators);
        }
    }

    private static void AccumulateMatch(MatchGroup match, Dictionary<int, StandingAccumulator> accumulators)
    {
        if (match.HomeTeam == null || match.AwayTeam == null)
        {
            return;
        }

        if (!IsMatchPlayed(match))
        {
            return;
        }

        if (!accumulators.TryGetValue(match.HomeTeam.Id, out StandingAccumulator? home)
            || !accumulators.TryGetValue(match.AwayTeam.Id, out StandingAccumulator? away))
        {
            return;
        }

        home.RegisterScored(match.HomeGoals, match.AwayGoals);
        away.RegisterScored(match.AwayGoals, match.HomeGoals);
    }

    private int ResolveTieBreakerSeed()
    {
        TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
        return settings?.CrossingsSeed ?? DefaultTieBreakerSeed;
    }

    private static Dictionary<int, int> BuildTieBreakerMap(IReadOnlyCollection<StandingAccumulator> rows, int seed)
    {
        Random random = new Random(seed);
        return rows.ToDictionary(row => row.Team.Id, _ => random.Next());
    }

    private static IEnumerable<StandingAccumulator> OrderStandings(
        IReadOnlyCollection<StandingAccumulator> rows,
        IReadOnlyDictionary<int, int> tieBreakers)
    {
        return rows
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenByDescending(row => tieBreakers[row.Team.Id])
            .ThenBy(row => row.Team.Id);
    }

    private IReadOnlyCollection<MatchGroup> GetAllGroupMatches()
    {
        return _matchRepository.GetAll().OfType<MatchGroup>().ToList();
    }

    private static bool IsMatchPlayed(Match match)
    {
        return match.HasResult;
    }

    private sealed class StandingAccumulator
    {
        public StandingAccumulator(Team team)
        {
            Team = team;
        }

        public Team Team { get; }
        public int GamesPlayed { get; private set; }
        public int Wins { get; private set; }
        public int Draws { get; private set; }
        public int Losses { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }

        public int GoalDifference => GoalsFor - GoalsAgainst;

        public int Points => Wins * PointsForWin + Draws * PointsForDraw;

        public void RegisterScored(int goalsScored, int goalsConceded)
        {
            GamesPlayed++;
            GoalsFor += goalsScored;
            GoalsAgainst += goalsConceded;

            if (goalsScored > goalsConceded)
            {
                Wins++;
            }
            else if (goalsScored == goalsConceded)
            {
                Draws++;
            }
            else
            {
                Losses++;
            }
        }

        public GroupStandingDto ToDto()
        {
            return new GroupStandingDto(
                Team,
                GamesPlayed,
                Wins,
                Draws,
                Losses,
                GoalsFor,
                GoalsAgainst,
                GoalDifference,
                Points);
        }
    }
}
