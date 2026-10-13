using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class GroupStandingsService : IGroupStandingsService
{
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;
    private const int DefaultTieBreakerSeed = 0;
    private const string GroupNotFoundMessage = "No se encontro el grupo.";

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
        Dictionary<int, int> tieBreakers = BuildTieBreakerMap(rows, ResolveTieBreakerSeed(), ResolveGroupIndex(group.Name));

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

    // Indice del grupo en el orden alfabetico ordinal de todos los grupos, replicando el groupIndex
    // que usa GroupQualifier; es parte de la semilla por-equipo del desempate compartido (H8).
    private int ResolveGroupIndex(string groupName)
    {
        List<string> orderedNames = _groupRepository.GetAll()
            .Select(group => group.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
        return orderedNames.IndexOf(groupName);
    }

    private static Dictionary<int, int> BuildTieBreakerMap(IReadOnlyCollection<StandingAccumulator> rows, int seed, int groupIndex)
    {
        // Desempate por-equipo compartido con GroupQualifier (H8), no un Random por fila.
        return rows.ToDictionary(row => row.Team.Id, row => GroupTieBreaker.Compute(seed, groupIndex, row.Team.Id));
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
