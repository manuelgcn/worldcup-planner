using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class FixtureOrchestrator : IFixtureOrchestrator
{
    private const int RequiredTeamCount = 48;
    private const int MinimumStadiumCount = 4;
    private const int GroupStageJourneyCount = 3;
    private const string FixtureAlreadyGeneratedMessage = "El fixture ya fue generado. Use RegenerateGroupStage en su lugar.";
    private const string FixtureNotGeneratedMessage = "El fixture aun no fue generado.";
    private const string KnockoutAlreadyGeneratedMessage = "No se puede regenerar el fixture: la fase eliminatoria ya existe.";
    private const string MissingSettingsMessage = "La configuracion del torneo no esta definida.";
    private const string MissingTeamsMessage = "El fixture requiere exactamente 48 equipos.";
    private const string MissingStadiumsMessage = "El fixture requiere al menos 4 estadios.";
    private const string ClearFixtureDetails = "Fixture anterior limpiado antes de la regeneracion.";
    private const string DeleteFixtureDetails = "Fixture eliminado: se removieron grupos, jornadas, partidos de grupo y fase eliminatoria.";

    private readonly ITeamRepository _teamRepository;
    private readonly IStadiumRepository _stadiumRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IEliminationMatchRepository _eliminationRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ISimulationEngineRegistry _simulationEngineRegistry;
    private readonly IRankingService _rankingService;

    public FixtureOrchestrator(
        ITeamRepository teamRepository,
        IStadiumRepository stadiumRepository,
        ITournamentSettingsRepository settingsRepository,
        IGroupRepository groupRepository,
        IJourneyRepository journeyRepository,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        IAuditLogService auditLogService,
        ISimulationEngineRegistry simulationEngineRegistry,
        IRankingService rankingService)
    {
        _teamRepository = teamRepository;
        _stadiumRepository = stadiumRepository;
        _settingsRepository = settingsRepository;
        _groupRepository = groupRepository;
        _journeyRepository = journeyRepository;
        _matchRepository = matchRepository;
        _eliminationRepository = eliminationRepository;
        _auditLogService = auditLogService;
        _simulationEngineRegistry = simulationEngineRegistry;
        _rankingService = rankingService;
    }

    public bool IsFixtureGenerated()
    {
        return _groupRepository.Any();
    }

    public bool IsKnockoutGenerated()
    {
        return _eliminationRepository.Any();
    }

    public void GenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
        EnsureFixtureNotYetGenerated();
        GenerationPlan plan = PrepareGenerationPlan(fixtureSeedOverride);
        ApplySimulationEngineSelection(plan.Settings, selectedEngineName);

        BuildAndPersist(plan);

        AuditGenerated(plan.EffectiveSeed, plan.Settings.SelectedSimulationEngine, performedByEmail);
    }

    public void RegenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
        EnsureFixtureGenerated();
        EnsureKnockoutNotYetGenerated();
        GenerationPlan plan = PrepareGenerationPlan(fixtureSeedOverride);
        ApplySimulationEngineSelection(plan.Settings, selectedEngineName);

        RevertRankingOfPlayedMatches();
        ClearGroupStageStorage();
        AuditCleared(performedByEmail);

        // El plan se armo antes de revertir (para validar sin tocar nada); los bombos salen del
        // ranking, asi que se arman con los equipos releidos tras la reversion.
        BuildAndPersist(plan with { Teams = LoadTeams() });

        AuditRegenerated(plan.EffectiveSeed, plan.Settings.SelectedSimulationEngine, performedByEmail);
    }

    // Valida el motor contra el registry (fail-fast antes de tocar el storage), lo persiste en
    // TournamentSettings antes de generar el fixture.
    // Con null se mantiene el motor ya persistido.
    private void ApplySimulationEngineSelection(TournamentSettings settings, string? selectedEngineName)
    {
        if (selectedEngineName == null)
        {
            return;
        }

        IMatchSimulationEngine engine = _simulationEngineRegistry.Resolve(selectedEngineName);
        settings.SelectSimulationEngine(engine.Name);
        _settingsRepository.Update(settings);
    }

    public void DeleteGroupStage(string? performedByEmail)
    {
        // Borrado total del torneo: se elimina la fase 2 (knockout) ANTES que los partidos de grupo
        // porque las eliminatorias referencian a los partidos de grupo como fuente (FK Matches->Matches).
        RevertRankingOfPlayedMatches();
        _eliminationRepository.Clear();
        ClearGroupStageStorage();

        AuditDeleted(performedByEmail);
    }

    private GenerationPlan PrepareGenerationPlan(int? fixtureSeedOverride)
    {
        TournamentSettings settings = LoadSettings();
        IReadOnlyCollection<Team> teams = LoadTeams();
        IReadOnlyCollection<Stadium> stadiums = LoadStadiums();
        int effectiveSeed = fixtureSeedOverride ?? settings.FixtureSeed;
        return new GenerationPlan(teams, stadiums, settings, effectiveSeed);
    }

    private void BuildAndPersist(GenerationPlan plan)
    {
        IReadOnlyCollection<Group> groups = BuildGroups(plan);
        PersistGroups(groups);
        PersistJourneys();
        PersistMatches(groups, plan.Stadiums, plan.Settings);
    }

    private static IReadOnlyCollection<Group> BuildGroups(GenerationPlan plan)
    {
        FixtureService fixtureService = new FixtureService(plan.Teams, plan.Stadiums, plan.EffectiveSeed);
        return fixtureService.CreateFixture();
    }

    private void PersistGroups(IReadOnlyCollection<Group> groups)
    {
        foreach (Group group in groups)
        {
            _groupRepository.Add(group);
        }
    }

    private void PersistJourneys()
    {
        for (int journeyNumber = 1; journeyNumber <= GroupStageJourneyCount; journeyNumber++)
        {
            _journeyRepository.Add(Journey.CreateInstance(journeyNumber));
        }
    }

    private void PersistMatches(
        IReadOnlyCollection<Group> groups,
        IReadOnlyCollection<Stadium> stadiums,
        TournamentSettings settings)
    {
        GroupFixtureGenerator matchGenerator = new GroupFixtureGenerator();
        List<Match> matches = matchGenerator.GenerateMatches(
            groups.ToList(),
            stadiums.ToList(),
            settings.StartDate,
            settings.DaysBetweenJourneys,
            settings.MaxMatchesPerDay);

        foreach (Match match in matches)
        {
            _matchRepository.Add(match);
        }
    }

    // Cada resultado cambio el ranking guardado en los equipos y el sorteo arma los bombos por
    // ranking: sin revertirlo, la misma semilla ya no daria los mismos grupos. GetAll trae grupos
    // y eliminatorias (TPH) con los equipos compartidos, asi que las reversiones se acumulan bien.
    private void RevertRankingOfPlayedMatches()
    {
        foreach (Match match in _matchRepository.GetAll().Where(match => match.RankingApplied))
        {
            _rankingService.RevertMatchResult(match);
        }
    }

    private void ClearGroupStageStorage()
    {
        _matchRepository.Clear();
        _journeyRepository.Clear();
        _groupRepository.Clear();
    }

    private void EnsureFixtureNotYetGenerated()
    {
        if (IsFixtureGenerated())
        {
            throw new FixtureOrchestratorException(FixtureAlreadyGeneratedMessage);
        }
    }

    private void EnsureFixtureGenerated()
    {
        if (!IsFixtureGenerated())
        {
            throw new FixtureOrchestratorException(FixtureNotGeneratedMessage);
        }
    }

    private void EnsureKnockoutNotYetGenerated()
    {
        if (IsKnockoutGenerated())
        {
            throw new FixtureOrchestratorException(KnockoutAlreadyGeneratedMessage);
        }
    }

    private TournamentSettings LoadSettings()
    {
        TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
        if (settings == null)
        {
            throw new FixtureOrchestratorException(MissingSettingsMessage);
        }
        return settings;
    }

    private IReadOnlyCollection<Team> LoadTeams()
    {
        IReadOnlyCollection<Team> teams = _teamRepository.GetAll();
        if (teams.Count != RequiredTeamCount)
        {
            throw new FixtureOrchestratorException(MissingTeamsMessage);
        }
        return teams;
    }

    private IReadOnlyCollection<Stadium> LoadStadiums()
    {
        IReadOnlyCollection<Stadium> stadiums = _stadiumRepository.GetAll();
        if (stadiums.Count < MinimumStadiumCount)
        {
            throw new FixtureOrchestratorException(MissingStadiumsMessage);
        }
        return stadiums;
    }

    private void AuditGenerated(int seed, string engineName, string? performedByEmail)
    {
        _auditLogService.LogPerformedBy(
            AuditActions.GenerateFixture,
            $"Fixture generated with seed {seed} and simulation engine {engineName}.",
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    private void AuditCleared(string? performedByEmail)
    {
        _auditLogService.LogPerformedBy(
            AuditActions.ClearFixture,
            ClearFixtureDetails,
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    private void AuditRegenerated(int seed, string engineName, string? performedByEmail)
    {
        _auditLogService.LogPerformedBy(
            AuditActions.RegenerateFixture,
            $"Fixture regenerated with seed {seed} and simulation engine {engineName}.",
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    private void AuditDeleted(string? performedByEmail)
    {
        _auditLogService.LogPerformedBy(
            AuditActions.DeleteFixture,
            DeleteFixtureDetails,
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    private sealed record GenerationPlan(
        IReadOnlyCollection<Team> Teams,
        IReadOnlyCollection<Stadium> Stadiums,
        TournamentSettings Settings,
        int EffectiveSeed);
}
