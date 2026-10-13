using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class TournamentSimulationService : ITournamentSimulationService
{
    private readonly IGroupRepository groupRepository;
    private readonly IMatchRepository matchRepository;
    private readonly IEliminationMatchRepository eliminationMatchRepository;
    private readonly ISimulationEngineRegistry simulationEngineRegistry;
    private readonly IAuditLogService auditLogService;
    private readonly ITournamentSettingsRepository tournamentSettingsRepository;
    private readonly IKnockoutFixtureGenerator knockoutFixtureGenerator;
    private readonly INotificationService notificationService;
    private readonly IRankingService rankingService;

    private const string FixtureNotGeneratedMessage = "El fixture aún no ha sido generado";
    private const string GroupStageAlreadyPlayedMessage = "La fase de grupos ya fue jugada";

    public TournamentSimulationService(
        IGroupRepository groupRepository,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationMatchRepository,
        ISimulationEngineRegistry simulationEngineRegistry,
        IAuditLogService auditLogService,
        ITournamentSettingsRepository tournamentSettingsRepository,
        IKnockoutFixtureGenerator knockoutFixtureGenerator,
        INotificationService notificationService,
        IRankingService rankingService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(rankingService);
        this.groupRepository = groupRepository;
        this.matchRepository = matchRepository;
        this.eliminationMatchRepository = eliminationMatchRepository;
        this.simulationEngineRegistry = simulationEngineRegistry;
        this.auditLogService = auditLogService;
        this.tournamentSettingsRepository = tournamentSettingsRepository;
        this.knockoutFixtureGenerator = knockoutFixtureGenerator;
        this.notificationService = notificationService;
        this.rankingService = rankingService;
    }

    public void SimulateGroupStage(int seed, string? performedByEmail)
    {
        IMatchSimulationEngine engine = ResolveSelectedEngine(seed);
        RunGroupStageSimulation(seed, engine);
        LogGroupStageSimulation(seed, engine.Name, performedByEmail);
    }

    // Accion "todo en uno" deliberada y separada: simula la fase de grupos y genera
    // los cruces de fase 2 en un solo paso. La usa unicamente la pagina Simulador
    // (Simulator.razor), que avisa explicitamente al usuario. El flujo manual paso a
    // paso (Fixture.razor) NO la usa: alli se simula la fase de grupos con
    // SimulateGroupStage y la fase 2 se genera con un boton aparte.
    public void SimulateGroupStageAndGenerateKnockout(int seed, string? performedByEmail)
    {
        // No re-simular una fase de grupos ya jugada entera; si esta a medias, solo se simulan los
        // partidos sin resultado. La regla vive en el servicio; la UI ofrece una primera barrera (Simulator.razor).
        EnsureGroupStageNotAlreadyPlayed();

        TournamentSettings settings = ResolveTournamentSettings(seed);
        IMatchSimulationEngine engine = simulationEngineRegistry.Resolve(settings.SelectedSimulationEngine);

        (List<Group> groups, List<Match> groupStageMatches) = RunGroupStageSimulation(seed, engine);

        QualifiedTeams qualifiedTeams = ClassifyQualifiedTeams(groups, groupStageMatches, settings);

        GenerateAndPersistKnockoutPhase(qualifiedTeams);

        LogSimulation(seed, engine.Name, performedByEmail);
    }

    // Simula los partidos de grupos sin resultado (sin generar la fase 2) y devuelve los grupos y
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

    // Considera "ya jugada" la fase de grupos cuando existe al menos un partido y TODOS tienen
    // resultado. Si todavia no hay fixture (0 partidos) no aplica esta regla: ese caso lo cubre
    // ValidateFixtureGenerated con su propio mensaje.
    private void EnsureGroupStageNotAlreadyPlayed()
    {
        List<MatchGroup> groupMatches = matchRepository.GetAll().OfType<MatchGroup>().ToList();
        if (groupMatches.Count > 0 && groupMatches.All(match => match.HasResult))
        {
            throw new MatchServicesException(GroupStageAlreadyPlayedMessage);
        }
    }

    // Solo los partidos sin resultado: lo que ya se cargo a mano (y su Elo) se respeta.
    private void SimulateMatches(List<Match> matches, int seed, IMatchSimulationEngine engine)
    {
        List<Match> pending = matches.Where(match => !match.HasResult).ToList();
        foreach (Match match in pending)
        {
            engine.Simulate(match, seed);
            ApplyRankingUpdate(match);
            matchRepository.Update(match);
        }

        notificationService.NotifyMatchesSimulated(pending);
    }

    private void ApplyRankingUpdate(Match match)
    {
        rankingService.ApplyMatchResultOnce(match);
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

    private void LogSimulation(int seed, string engineName, string? performedByEmail)
    {
        auditLogService.LogPerformedBy(
            AuditActions.SimulateGroupStageAndGenerateKnockout,
            $"Group stage simulated with engine {engineName} and seed {seed}; knockout stage generated.",
            performedByEmail,
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
