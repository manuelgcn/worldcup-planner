using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class GroupStandingsService : IGroupStandingsService
{
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

        List<Match> matches = _matchRepository.GetByGroup(group.Name);
        return Rank(group, matches, ResolveTieBreakerSeed(), GroupIndex(_groupRepository.GetAll(), group.Name));
    }

    // Una lectura de grupos, una de settings y una de partidos para todas las tablas (antes eran ~4 por grupo).
    public List<GroupStandingDto> GetStandingsForAllGroups()
    {
        IReadOnlyCollection<Group> groups = _groupRepository.GetAll();
        int seed = ResolveTieBreakerSeed();
        ILookup<string, MatchGroup> matchesByGroup = GetAllGroupMatches()
            .Where(match => match.Group != null)
            .ToLookup(match => match.Group!.Name);

        return groups
            .OrderBy(group => group.Name)
            .SelectMany(group => Rank(group, matchesByGroup[group.Name], seed, GroupIndex(groups, group.Name)))
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

    private int ResolveTieBreakerSeed()
    {
        TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
        return settings?.CrossingsSeed ?? DefaultTieBreakerSeed;
    }

    // Indice del grupo en el orden alfabetico ordinal de todos los grupos, replicando el groupIndex
    // que usa GroupQualifier; es parte de la semilla por-equipo del desempate compartido (H8).
    private static int GroupIndex(IEnumerable<Group> groups, string groupName)
    {
        List<string> orderedNames = groups
            .Select(group => group.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
        return orderedNames.IndexOf(groupName);
    }

    private static List<GroupStandingDto> Rank(Group group, IEnumerable<Match> matches, int seed, int groupIndex)
    {
        return GroupQualifier.RankGroup(group, matches, seed, groupIndex)
            .Select(ToDto)
            .ToList();
    }

    private IReadOnlyCollection<MatchGroup> GetAllGroupMatches()
    {
        return _matchRepository.GetAll().OfType<MatchGroup>().ToList();
    }

    private static bool IsMatchPlayed(Match match)
    {
        return match.HasResult;
    }

    private static GroupStandingDto ToDto(GroupStanding standing)
    {
        return new GroupStandingDto(
            standing.Team,
            standing.GamesPlayed,
            standing.Wins,
            standing.Draws,
            standing.Losses,
            standing.GoalsFor,
            standing.GoalsAgainst,
            standing.GoalDifference,
            standing.Points);
    }
}
