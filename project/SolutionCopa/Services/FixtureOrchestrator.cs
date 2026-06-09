using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class FixtureOrchestrator : IFixtureOrchestrator
{
    private const int RequiredTeamCount = 48;
    private const int MinimumStadiumCount = 4;
    private const int GroupStageJourneyCount = 3;
    private const string FixtureAlreadyGeneratedMessage = "Fixture is already generated. Use RegenerateGroupStage instead.";
    private const string FixtureNotGeneratedMessage = "Fixture has not been generated yet.";
    private const string KnockoutAlreadyGeneratedMessage = "Cannot regenerate fixture: knockout phase already exists.";
    private const string MissingSettingsMessage = "Tournament settings are not configured.";
    private const string MissingTeamsMessage = "Fixture requires exactly 48 teams.";
    private const string MissingStadiumsMessage = "Fixture requires at least 4 stadiums.";
    private const string ClearFixtureDetails = "Previous fixture cleared before regeneration.";
    private const string DeleteFixtureDetails = "Fixture deleted: groups, journeys, group matches and knockout removed.";

    private readonly ITeamRepository _teamRepository;
    private readonly IStadiumRepository _stadiumRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IEliminationMatchRepository _eliminationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly ISimulationEngineRegistry _simulationEngineRegistry;

    public FixtureOrchestrator(
        ITeamRepository teamRepository,
        IStadiumRepository stadiumRepository,
        ITournamentSettingsRepository settingsRepository,
        IGroupRepository groupRepository,
        IJourneyRepository journeyRepository,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        ISimulationEngineRegistry simulationEngineRegistry)
    {
        _teamRepository = teamRepository;
        _stadiumRepository = stadiumRepository;
        _settingsRepository = settingsRepository;
        _groupRepository = groupRepository;
        _journeyRepository = journeyRepository;
        _matchRepository = matchRepository;
        _eliminationRepository = eliminationRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _simulationEngineRegistry = simulationEngineRegistry;
    }

    public bool IsFixtureGenerated()
    {
        return _groupRepository.GetAll().Count > 0;
    }

    public bool IsKnockoutGenerated()
    {
        return _eliminationRepository.GetAll().Count > 0;
    }

    public void GenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
        EnsureFixtureNotYetGenerated();
        GenerationPlan plan = PrepareGenerationPlan(fixtureSeedOverride);
        ApplySimulationEngineSelection(plan.Settings, selectedEngineName);

        ExecuteAndCommit(plan);

        AuditGenerated(plan.EffectiveSeed, plan.Settings.SelectedSimulationEngine, performedByEmail);
    }

    public void RegenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
        EnsureFixtureGenerated();
        EnsureKnockoutNotYetGenerated();
        GenerationPlan plan = PrepareGenerationPlan(fixtureSeedOverride);
        ApplySimulationEngineSelection(plan.Settings, selectedEngineName);

        ClearGroupStageStorage();
        AuditCleared(performedByEmail);

        ExecuteAndCommit(plan);

        AuditRegenerated(plan.EffectiveSeed, plan.Settings.SelectedSimulationEngine, performedByEmail);
    }

    // Valida el motor contra el registry (fail-fast antes de tocar el storage), lo persiste en
    // TournamentSettings y deja el Update dentro del mismo Commit que la generacion del fixture.
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
        _eliminationRepository.Clear();
        ClearGroupStageStorage();
        _unitOfWork.Commit();

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

    private void ExecuteAndCommit(GenerationPlan plan)
    {
        IReadOnlyCollection<Group> groups = BuildGroups(plan);
        PersistGroups(groups);
        PersistJourneys();
        PersistMatches(groups, plan.Stadiums, plan.Settings);
        _unitOfWork.Commit();
    }

    private static IReadOnlyCollection<Group> BuildGroups(GenerationPlan plan)
    {
        FixtureService fixtureService = new FixtureService(plan.Teams, plan.Stadiums, plan.EffectiveSeed, auditLogService: null);
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
            settings.StartDate);

        foreach (Match match in matches)
        {
            _matchRepository.Add(match);
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
