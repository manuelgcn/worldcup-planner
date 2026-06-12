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
    ISimulationEngineRegistry simulationEngineRegistry,
    IAuditLogService auditLogService,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IKnockoutFixtureGenerator knockoutFixtureGenerator,
    INotificationService? notificationService = null,
    IRankingService? rankingService = null) : ITournamentSimulationService
{
    private const string FixtureNotGeneratedMessage = "El fixture aún no ha sido generado";

    public void SimulateGroupStage(int seed, string? performedByEmail)
    {
        IMatchSimulationEngine engine = ResolveSelectedEngine(seed);
        RunGroupStageSimulation(seed, engine);
        LogGroupStageSimulation(seed, engine.Name, performedByEmail);
        unitOfWork.Commit();
    }

    public void SimulateGroupStageAndGenerateKnockout(int seed)
    {
        TournamentSettings settings = ResolveTournamentSettings(seed);
        IMatchSimulationEngine engine = simulationEngineRegistry.Resolve(settings.SelectedSimulationEngine);

        (List<Group> groups, List<Match> groupStageMatches) = RunGroupStageSimulation(seed, engine);

        QualifiedTeams qualifiedTeams = ClassifyQualifiedTeams(groups, groupStageMatches, settings);

        GenerateAndPersistKnockoutPhase(qualifiedTeams);

        LogSimulation(engine.Name);
        unitOfWork.Commit();
    }

    // Simula los 72 partidos de la fase de grupos (sin generar la fase 2) y devuelve los grupos y
    // sus partidos, para que el llamador pueda opcionalmente clasificar y armar el knockout.
    private (List<Group> Groups, List<Match> Matches) RunGroupStageSimulation(int seed, IMatchSimulationEngine engine)
    {
        List<Group> groups = GetOrderedGroups();
        List<Match> groupStageMatches = GetGroupStageMatches(groups);

        ValidateFixtureGenerated(groupStageMatches);

        SimulateMatches(groupStageMatches, seed, engine);

        return (groups, groupStageMatches);
    }

    private IMatchSimulationEngine ResolveSelectedEngine(int seed)
    {
        TournamentSettings settings = ResolveTournamentSettings(seed);
        return simulationEngineRegistry.Resolve(settings.SelectedSimulationEngine);
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

    private void SimulateMatches(List<Match> matches, int seed, IMatchSimulationEngine engine)
    {
        foreach (Match match in matches)
        {
            engine.Simulate(match, seed);
            ApplyRankingUpdate(match);
            matchRepository.Update(match);
            notificationService?.NotifyMatchSimulated(match);
        }
    }

    // Misma orquestacion que MatchService.ApplyRankingUpdate: deriva la fase del tipo de
    // partido y respeta el flag persistido RankingApplied para no recalcular el Elo. Se
    // invoca ANTES del Update del repositorio para que el flag viaje en el mismo guardado.
    private void ApplyRankingUpdate(Match match)
    {
        if (rankingService == null || match.RankingApplied)
        {
            return;
        }

        MatchPhase phase = match is MatchGroup ? MatchPhase.GroupStage : MatchPhase.Knockout;
        rankingService.ApplyMatchResult(match, phase);
        match.MarkRankingApplied();
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

    private void LogSimulation(string engineName)
    {
        auditLogService.Log(
            AuditActions.SimulateGroupStageAndGenerateKnockout,
            $"La fase de grupos fue simulada con el motor {engineName} y la fase 2 fue generada.",
            affectedEntityType: nameof(TournamentSimulationService));
    }

    private void LogGroupStageSimulation(int seed, string engineName, string? performedByEmail)
    {
        auditLogService.LogPerformedBy(
            AuditActions.SimulateGroupStage,
            $"Group stage simulated with engine {engineName} and seed {seed}.",
            performedByEmail,
            affectedEntityType: nameof(TournamentSimulationService));
    }
}
