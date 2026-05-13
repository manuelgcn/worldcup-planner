using DataAccess.Interfaces;
using ProjectCopa;
using ProjectCopa.Models;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class TournamentSimulationService(
    IGroupRepository groupRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    IMatchSimulationEngine matchSimulationEngine,
    IAuditLogService auditLogService,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IStadiumRepository stadiumRepository,
    IEliminationMatchRepository eliminationMatchRepository) : ITournamentSimulationService
{
    private const string FixtureNotGeneratedMessage = "El fixture aún no ha sido generado";
    private const string AuditLogMessage = "La fase de grupos fue simulada y la fase 2 fue generada.";

    public void SimulateGroupStageAndGenerateKnockout(int seed)
    {
        List<Group> groups = GetOrderedGroups();
        List<Match> groupStageMatches = GetGroupStageMatches(groups);

        ValidateFixtureGenerated(groupStageMatches);

        SimulateMatches(groupStageMatches, seed);

        TournamentSettings settings = ResolveTournamentSettings(seed);
        List<(Team Team, string Group)> qualifiedTeamsWithGroup = ClassifyQualifiedTeamsWithGroup(groups, groupStageMatches, settings);

        GenerateAndSimulateEliminationPhase(qualifiedTeamsWithGroup, settings);

        LogSimulation();
    }

    private List<Group> GetOrderedGroups()
    {
        return groupRepository.GetAll()
            .OrderBy(group => group.Name)
            .ToList();
    }

    private List<Match> GetGroupStageMatches(List<Group> groups)
    {
        List<Match> groupStageMatches = new List<Match>();
        
        foreach (Group group in groups)
        {
            List<Match> matchesForGroup = matchRepository.GetByGroup(group.Name)
                .OrderBy(match => match.MatchDate)
                .ThenBy(match => match.MatchId)
                .ToList();

            groupStageMatches.AddRange(matchesForGroup);
        }

        return groupStageMatches;
    }

    private void ValidateFixtureGenerated(List<Match> matches)
    {
        if (matches.Count == 0)
        {
            throw new MatchServicesException(FixtureNotGeneratedMessage);
        }
    }

    private void SimulateMatches(List<Match> matches, int seed)
    {
        foreach (Match match in matches)
        {
            matchSimulationEngine.Simulate(match, seed);
            matchRepository.Update(match);
        }
    }

    private List<(Team Team, string Group)> ClassifyQualifiedTeamsWithGroup(List<Group> groups, List<Match> matches, TournamentSettings settings)
    {
        GroupQualifier qualifier = new GroupQualifier();
        QualifiedTeams qualifiedTeams = qualifier.ClassifyTeams(groups, matches, settings.CrossingsSeed);

        Dictionary<int, string> teamGroupMap = groups
            .SelectMany(group => group.Teams.Select(team => new { team.Id, group.Name }))
            .ToDictionary(entry => entry.Id, entry => entry.Name);

        List<(Team Team, string Group)> qualifiedWithGroups = new List<(Team Team, string Group)>();
        AddTeamsWithGroup(qualifiedWithGroups, qualifiedTeams.TopEightFirsts, teamGroupMap);
        AddTeamsWithGroup(qualifiedWithGroups, qualifiedTeams.RemainingFourFirsts, teamGroupMap);
        AddTeamsWithGroup(qualifiedWithGroups, qualifiedTeams.TopEightSeconds, teamGroupMap);
        AddTeamsWithGroup(qualifiedWithGroups, qualifiedTeams.WorstFourSeconds, teamGroupMap);
        AddTeamsWithGroup(qualifiedWithGroups, qualifiedTeams.BestEightThirds, teamGroupMap);

        return qualifiedWithGroups;
    }

    private void GenerateAndSimulateEliminationPhase(List<(Team Team, string Group)> qualifiedTeamsWithGroup, TournamentSettings settings)
    {
        Fase2 fase2 = new Fase2(
            matchSimulationEngine,
            matchRepository,
            tournamentSettingsRepository,
            unitOfWork,
            auditLogService);

        fase2.OrchestrateAndSimulateEliminationPhase(
            qualifiedTeamsWithGroup,
            stadiumRepository.GetAll(),
            settings,
            eliminationMatchRepository,
            settings.CrossingsSeed);
    }

    private TournamentSettings ResolveTournamentSettings(int seed)
    {
        TournamentSettings? persistedSettings = tournamentSettingsRepository.GetAll().FirstOrDefault();
        if (persistedSettings != null)
        {
            return persistedSettings;
        }

        return new TournamentSettings(
            id: 0,
            startDate: new DateTime(2026, 6, 1),
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: seed,
            crossingsSeed: seed,
            simulationSeed: seed,
            completionSeed: seed);
    }

    private static void AddTeamsWithGroup(
        List<(Team Team, string Group)> target,
        IEnumerable<Team> teams,
        IReadOnlyDictionary<int, string> teamGroupMap)
    {
        foreach (Team team in teams)
        {
            if (!teamGroupMap.TryGetValue(team.Id, out string? groupName))
            {
                throw new InvalidOperationException($"No se encontró grupo para el equipo {team.Id}.");
            }

            target.Add((team, groupName));
        }
    }

    private void LogSimulation()
    {
        auditLogService.Log(
            AuditActions.SimulateGroupStageAndGenerateKnockout,
            AuditLogMessage,
            affectedEntityType: nameof(TournamentSimulationService));
    }
}
