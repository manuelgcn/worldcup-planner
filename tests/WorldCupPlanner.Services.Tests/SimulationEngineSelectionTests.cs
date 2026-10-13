using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

// Wiring de la Fase 4: cada consumidor resuelve el motor via ISimulationEngineRegistry
// usando TournamentSettings.SelectedSimulationEngine, y audita el nombre del motor usado.
[TestClass]
public sealed class SimulationEngineSelectionTests
{
    private const string ProbabilisticName = "Probabilístico";
    private const string PureRandomName = "Aleatorio Puro";
    private const int Seed = 42;
    private const int SettingsId = 1;
    private const int MatchId = 1;
    private const string AdminEmail = "admin@copa.com";

    private MockMatchSimulationEngine _defaultEngineSpy = null!;
    private MockMatchSimulationEngine _selectedEngineSpy = null!;
    private SimulationEngineRegistry _registry = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private MockAuditLogRepository _auditLogRepository = null!;
    private AuditLogService _auditLogService = null!;
    private MockUnitOfWork _unitOfWork = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _defaultEngineSpy = new MockMatchSimulationEngine { Name = ProbabilisticName };
        _selectedEngineSpy = new MockMatchSimulationEngine { Name = PureRandomName };
        _registry = new SimulationEngineRegistry(
            new IMatchSimulationEngine[] { _defaultEngineSpy, _selectedEngineSpy });
        _settingsRepository = new MockTournamentSettingsRepository();
        _auditLogRepository = new MockAuditLogRepository();
        _auditLogService = AuditLogService.CreateInstance(_auditLogRepository);
        _unitOfWork = new MockUnitOfWork();

        TournamentSettings settings = new TournamentSettings(
            SettingsId, new DateTime(2026, 6, 1), 3, 3, Seed, Seed, Seed, Seed);
        settings.SelectSimulationEngine(PureRandomName);
        _settingsRepository.Add(settings);
    }

    [TestMethod]
    public void TournamentSimulationService_SimulateGroupStage_UsesEngineSelectedInSettings()
    {
        TournamentSimulationService service = CreateTournamentSimulationService();

        service.SimulateGroupStage(Seed, AdminEmail);

        Assert.IsTrue(_selectedEngineSpy.SimulateCalls > 0);
        Assert.AreEqual(0, _defaultEngineSpy.SimulateCalls);
    }

    [TestMethod]
    public void TournamentSimulationService_SimulateGroupStage_AuditsEngineName()
    {
        TournamentSimulationService service = CreateTournamentSimulationService();

        service.SimulateGroupStage(Seed, AdminEmail);

        AuditLog log = _auditLogRepository.GetAll().First(entry => entry.Action == AuditActions.SimulateGroupStage);
        Assert.IsTrue(log.Details.Contains(PureRandomName), $"Expected engine name in details, got '{log.Details}'");
    }

    [TestMethod]
    public void MatchService_SimulateMatch_UsesEngineSelectedInSettings()
    {
        MatchService service = CreateMatchService();

        service.SimulateMatch(MatchId);

        Assert.AreEqual(1, _selectedEngineSpy.SimulateCalls);
        Assert.AreEqual(0, _defaultEngineSpy.SimulateCalls);
    }

    [TestMethod]
    public void MatchService_SimulateMatch_AuditsEngineName()
    {
        MatchService service = CreateMatchService();

        service.SimulateMatch(MatchId);

        AuditLog log = _auditLogRepository.GetAll().First(entry => entry.Action == AuditActions.SimulationExecuted);
        Assert.IsTrue(log.Details.Contains(PureRandomName), $"Expected engine name in details, got '{log.Details}'");
    }

    [TestMethod]
    public void KnockoutFixtureGenerator_SimulateEliminationMatches_UsesEngineSelectedInSettings()
    {
        MockMatchRepository matchRepository = new MockMatchRepository();
        matchRepository.Add(CreateGroupMatch(MatchId));
        KnockoutFixtureGenerator generator = new KnockoutFixtureGenerator(
            _registry,
            matchRepository,
            new MockEliminationMatchRepository(),
            new MockGroupRepository(),
            new MockStadiumRepository(),
            _settingsRepository,
            _unitOfWork,
            _auditLogService,
            new MockNotificationService(),
            new MockRankingService());

        generator.SimulateEliminationMatches(new[] { MatchId });

        Assert.AreEqual(1, _selectedEngineSpy.SimulateCalls);
        Assert.AreEqual(0, _defaultEngineSpy.SimulateCalls);
    }

    [TestMethod]
    public void KnockoutFixtureGenerator_SimulateEliminationMatches_AuditsEngineName()
    {
        MockMatchRepository matchRepository = new MockMatchRepository();
        matchRepository.Add(CreateGroupMatch(MatchId));
        KnockoutFixtureGenerator generator = new KnockoutFixtureGenerator(
            _registry,
            matchRepository,
            new MockEliminationMatchRepository(),
            new MockGroupRepository(),
            new MockStadiumRepository(),
            _settingsRepository,
            _unitOfWork,
            _auditLogService,
            new MockNotificationService(),
            new MockRankingService());

        generator.SimulateEliminationMatches(new[] { MatchId });

        AuditLog log = _auditLogRepository.GetAll().First(entry => entry.Action == AuditActions.SimulationExecuted);
        Assert.IsTrue(log.Details.Contains(PureRandomName), $"Expected engine name in details, got '{log.Details}'");
    }

    private TournamentSimulationService CreateTournamentSimulationService()
    {
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockMatchRepository matchRepository = new MockMatchRepository();
        SeedGroupWithMatches(groupRepository, matchRepository);

        return new TournamentSimulationService(
            groupRepository,
            matchRepository,
            new MockEliminationMatchRepository(),
            _unitOfWork,
            _registry,
            _auditLogService,
            _settingsRepository,
            new MockKnockoutFixtureGenerator(),
            new MockNotificationService(),
            new Moq.Mock<IRankingService>().Object);
    }

    private MatchService CreateMatchService()
    {
        MockMatchRepository matchRepository = new MockMatchRepository();
        matchRepository.Add(CreateGroupMatch(MatchId));

        return new MatchService(
            matchRepository,
            _settingsRepository,
            new StubMatchFactory(),
            _registry,
            _auditLogService,
            new Moq.Mock<IRankingService>().Object,
            new MockNotificationService(),
            _unitOfWork,
            new MockEliminationMatchRepository());
    }

    private static void SeedGroupWithMatches(MockGroupRepository groupRepository, MockMatchRepository matchRepository)
    {
        Group group = new Group("A");
        Team homeTeam = new Team(1, "Home Team", 1500, false, Confederation.CONCACAF);
        Team awayTeam = new Team(2, "Away Team", 1600, false, Confederation.UEFA);
        group.AddTeam(homeTeam);
        group.AddTeam(awayTeam);
        groupRepository.Seed(1, group);

        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        MatchGroup match = MatchGroup.CreateInstance(
            MatchId, new DateTime(2026, 6, 1), homeTeam, awayTeam, stadium, group, Journey.CreateInstance(1));
        matchRepository.Add(match);
        matchRepository.ResetCounters();
    }

    private static MatchGroup CreateGroupMatch(int matchId)
    {
        Team homeTeam = new Team(1, "Home Team", 1500, false, Confederation.CONCACAF);
        Team awayTeam = new Team(2, "Away Team", 1600, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        Group group = new Group("A");

        return MatchGroup.CreateInstance(
            matchId, new DateTime(2026, 6, 1), homeTeam, awayTeam, stadium, group, Journey.CreateInstance(1));
    }

    // Stub minimo: SimulateMatch no pasa por la fabrica, pero el constructor la exige.
    private sealed class StubMatchFactory : IMatchFactory
    {
        public Match Create(CreateMatchRequestDto request)
        {
            throw new NotSupportedException();
        }

        public void ApplyUpdate(Match match, UpdateMatchRequestDto request)
        {
            throw new NotSupportedException();
        }
    }
}
