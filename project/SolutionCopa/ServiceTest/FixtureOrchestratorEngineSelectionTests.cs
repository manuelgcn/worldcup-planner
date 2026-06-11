using DataAccess.Interfaces;
using ProjectCopa;
using Services;
using Services.Exceptions;
using Services.Interfaces;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

// Fase 4: GenerateGroupStage/RegenerateGroupStage reciben el nombre del motor elegido,
// lo validan contra el registry, lo persisten en TournamentSettings y lo auditan.
[TestClass]
public sealed class FixtureOrchestratorEngineSelectionTests
{
    private const string ProbabilisticName = "Probabilístico";
    private const string PureRandomName = "Aleatorio Puro";
    private const string UnknownEngineName = "Inexistente";
    private const int DefaultSettingsId = 1;
    private const int TotalExpectedTeams = 48;
    private const int RequiredStadiumCount = 4;
    private const string AdminEmail = "admin@copa.com";

    private MockTeamRepository _teamRepository = null!;
    private MockStadiumRepository _stadiumRepository = null!;
    private SpyTournamentSettingsRepository _settingsRepository = null!;
    private MockGroupRepository _groupRepository = null!;
    private MockJourneyRepository _journeyRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private MockEliminationMatchRepository _eliminationRepository = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private MockAuditLogRepository _auditRepository = null!;
    private FixtureOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void Setup()
    {
        _teamRepository = new MockTeamRepository();
        _stadiumRepository = new MockStadiumRepository();
        _settingsRepository = new SpyTournamentSettingsRepository();
        _groupRepository = new MockGroupRepository();
        _journeyRepository = new MockJourneyRepository();
        _matchRepository = new MockMatchRepository();
        _eliminationRepository = new MockEliminationMatchRepository();
        _unitOfWork = new MockUnitOfWork();
        _auditRepository = new MockAuditLogRepository();

        _settingsRepository.Add(new TournamentSettings(
            DefaultSettingsId, new DateTime(2026, 6, 1), 3, 3, 7, 0, 42, 0));
        SeedStadiums(RequiredStadiumCount);
        SeedTeams(TotalExpectedTeams);

        _orchestrator = new FixtureOrchestrator(
            _teamRepository,
            _stadiumRepository,
            _settingsRepository,
            _groupRepository,
            _journeyRepository,
            _matchRepository,
            _eliminationRepository,
            _unitOfWork,
            AuditLogService.CreateInstance(_auditRepository),
            new SimulationEngineRegistry(new IMatchSimulationEngine[]
            {
                new ProbabilisticSimulationEngine(),
                new PureRandomSimulationEngine()
            }));
    }

    [TestMethod]
    public void GenerateGroupStage_WithEngineName_PersistsSelectedEngine()
    {
        _orchestrator.GenerateGroupStage(null, PureRandomName, AdminEmail);

        TournamentSettings stored = _settingsRepository.GetById(DefaultSettingsId)!;
        Assert.AreEqual(PureRandomName, stored.SelectedSimulationEngine);
        Assert.IsTrue(_settingsRepository.UpdateCalls >= 1, "Expected settings Update to persist the engine.");
    }

    [TestMethod]
    public void GenerateGroupStage_WithEngineName_AuditsEngineInDetails()
    {
        _orchestrator.GenerateGroupStage(null, PureRandomName, AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateFixture);
        Assert.IsTrue(log.Details.Contains(PureRandomName), $"Expected engine name in details, got '{log.Details}'");
    }

    [TestMethod]
    public void GenerateGroupStage_WithNullEngineName_KeepsPersistedEngine()
    {
        _orchestrator.GenerateGroupStage(null, null, AdminEmail);

        TournamentSettings stored = _settingsRepository.GetById(DefaultSettingsId)!;
        Assert.AreEqual(ProbabilisticName, stored.SelectedSimulationEngine);
        Assert.AreEqual(0, _settingsRepository.UpdateCalls);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateFixture);
        Assert.IsTrue(log.Details.Contains(ProbabilisticName), $"Expected persisted engine in details, got '{log.Details}'");
    }

    [TestMethod]
    public void GenerateGroupStage_WithUnknownEngineName_ThrowsAndGeneratesNothing()
    {
        Assert.ThrowsException<SimulationEngineException>(
            () => _orchestrator.GenerateGroupStage(null, UnknownEngineName, AdminEmail));

        Assert.AreEqual(0, _groupRepository.GetAll().Count, "No groups should be persisted when the engine is invalid.");
        Assert.AreEqual(ProbabilisticName, _settingsRepository.GetById(DefaultSettingsId)!.SelectedSimulationEngine);
    }

    [TestMethod]
    public void RegenerateGroupStage_WithEngineName_PersistsSelectedEngineAndAuditsIt()
    {
        _orchestrator.GenerateGroupStage(null, null, AdminEmail);

        _orchestrator.RegenerateGroupStage(null, PureRandomName, AdminEmail);

        TournamentSettings stored = _settingsRepository.GetById(DefaultSettingsId)!;
        Assert.AreEqual(PureRandomName, stored.SelectedSimulationEngine);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.RegenerateFixture);
        Assert.IsTrue(log.Details.Contains(PureRandomName), $"Expected engine name in details, got '{log.Details}'");
    }

    private void SeedStadiums(int count)
    {
        for (int index = 0; index < count; index++)
        {
            Stadium stadium = new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 };
            _stadiumRepository.Add(stadium);
        }
    }

    private void SeedTeams(int count)
    {
        Confederation[] confederations = Enum.GetValues<Confederation>();
        const int teamsPerRankingTier = 12;
        for (int teamIndex = 0; teamIndex < count; teamIndex++)
        {
            int tier = teamIndex / teamsPerRankingTier;
            Team team = new Team(
                id: teamIndex + 1,
                name: $"Team-{teamIndex + 1:D2}",
                fifaRanking: 2000 - (tier * 100),
                isFictitious: false,
                confederation: confederations[teamIndex % confederations.Length]);
            _teamRepository.Add(team);
        }
    }

    // Doble con contador de Update para verificar que el motor elegido realmente se persiste.
    private sealed class SpyTournamentSettingsRepository : ITournamentSettingsRepository
    {
        private readonly Dictionary<int, TournamentSettings> _settings = new();

        public int UpdateCalls { get; private set; }

        public void Add(TournamentSettings settings)
        {
            _settings[settings.Id] = settings;
        }

        public void Update(TournamentSettings settings)
        {
            _settings[settings.Id] = settings;
            UpdateCalls++;
        }

        public void Delete(TournamentSettings settings)
        {
            _settings.Remove(settings.Id);
        }

        public TournamentSettings? GetById(int tournamentSettingsId)
        {
            return _settings.GetValueOrDefault(tournamentSettingsId);
        }

        public IReadOnlyCollection<TournamentSettings> GetAll()
        {
            return _settings.Values.ToList();
        }
    }
}
