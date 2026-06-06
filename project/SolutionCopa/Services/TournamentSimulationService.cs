using DataAccess.Interfaces;
using ProjectCopa;
using ProjectCopa.Models;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class TournamentSimulationService(
    IGroupRepository groupRepository,
    IMatchRepository matchRepository,
    IEliminationMatchRepository eliminationMatchRepository,
    IUnitOfWork unitOfWork,
    IMatchSimulationEngine matchSimulationEngine,
    IAuditLogService auditLogService,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IKnockoutFixtureGenerator knockoutFixtureGenerator) : ITournamentSimulationService
{
    private const string FixtureNotGeneratedMessage = "El fixture aún no ha sido generado";
    private const string AuditLogMessage = "La fase de grupos fue simulada y la fase 2 fue generada.";

    public void SimulateGroupStage(int seed, string? performedByEmail)
    {
        RunGroupStageSimulation(seed);
        unitOfWork.Commit();
    }

    public void SimulateGroupStageAndGenerateKnockout(int seed)
    {
        (List<Group> groups, List<Match> groupStageMatches) = RunGroupStageSimulation(seed);

        TournamentSettings settings = ResolveTournamentSettings(seed);
        QualifiedTeams qualifiedTeams = ClassifyQualifiedTeams(groups, groupStageMatches, settings);

        GenerateAndPersistKnockoutPhase(qualifiedTeams);

        LogSimulation();
        unitOfWork.Commit();
    }

    // Simula los 72 partidos de la fase de grupos (sin generar la fase 2) y devuelve los grupos y
    // sus partidos, para que el llamador pueda opcionalmente clasificar y armar el knockout.
    private (List<Group> Groups, List<Match> Matches) RunGroupStageSimulation(int seed)
    {
        List<Group> groups = GetOrderedGroups();
        List<Match> groupStageMatches = GetGroupStageMatches(groups);

        ValidateFixtureGenerated(groupStageMatches);

        SimulateMatches(groupStageMatches, seed);

        return (groups, groupStageMatches);
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

    private static QualifiedTeams ClassifyQualifiedTeams(List<Group> groups, List<Match> matches, TournamentSettings settings)
    {
        GroupQualifier qualifier = new GroupQualifier();
        return qualifier.ClassifyTeams(groups, matches, settings.CrossingsSeed);
    }

    private void GenerateAndPersistKnockoutPhase(QualifiedTeams qualifiedTeams)
    {
        List<Match> knockoutMatches = knockoutFixtureGenerator.GeneratePhase2(qualifiedTeams);
        foreach (Match match in knockoutMatches)
        {
            eliminationMatchRepository.Add(match);
        }
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

    private void LogSimulation()
    {
        auditLogService.Log(
            AuditActions.SimulateGroupStageAndGenerateKnockout,
            AuditLogMessage,
            affectedEntityType: nameof(TournamentSimulationService));
    }
}
