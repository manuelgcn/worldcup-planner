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
        return GroupQualifier.RankGroup(group, matches, ResolveTieBreakerSeed(), ResolveGroupIndex(group.Name))
            .Select(ToDto)
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
