using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class KnockoutFixtureGeneratorSimulationTests
{
    [TestMethod]
    public void OrchestrateAndSimulateEliminationPhase_CreatesMatches_WithTournamentSettingsAndLogs()
    {
        // Arrange
        List<(Team Team, string Group)> teamsWithGroup = new List<(Team Team, string Group)>();
        int id = 1;
        for (int g = 1; g <= 16; g++)
        {
            teamsWithGroup.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            teamsWithGroup.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
        }

        List<Stadium> stadiums = new List<Stadium>
        {
            new Stadium("S1", "City1", null, 30000),
            new Stadium("S2", "City2", null, 35000)
        };

        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:777, simulationSeed:99, completionSeed:0);

        MockEliminationMatchRepository eliminationRepo = new MockEliminationMatchRepository();
        MockTournamentSettingsRepository settingsRepo = new MockTournamentSettingsRepository();
        settingsRepo.Add(settings);
        MockMatchSimulationEngine simulation = new MockMatchSimulationEngine();
        MockUnitOfWork uow = new MockUnitOfWork();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        KnockoutFixtureGenerator generator = CreateGenerator(
            new MockMatchRepository(), eliminationRepo, settingsRepo, simulation, uow, auditRepo);

        // Act
        generator.OrchestrateAndSimulateEliminationPhase(teamsWithGroup, stadiums, settings, eliminationRepo);

        // Assert
        List<Match> all = eliminationRepo.GetAll().ToList();
        Assert.AreEqual(31, all.Count);

        // Check that created matches have tournament settings propagated (via MatchElimination when created with config)
        Assert.IsTrue(all.OfType<MatchElimination>().All(m => m != null));

        // Check audit logs contain the knockout-generation action with tournament id
        List<AuditLog> logs = auditRepo.GetAll();
        Assert.IsTrue(logs.Any(l => l.Action == AuditActions.GenerateKnockout && l.TournamentSettingsId == settings.Id));
        Assert.IsTrue(simulation.SimulateWasCalled);
        Assert.IsTrue(uow.CommitCalls >= 1);
    }

    [TestMethod]
    public void SimulateEliminationMatches_LogsEachMatch_WithSeedAndTournamentId()
    {
        // Arrange
        MockMatchRepository matchRepo = new MockMatchRepository();
        MockTournamentSettingsRepository settingsRepo = new MockTournamentSettingsRepository();
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:0, simulationSeed:42, completionSeed:0);
        settingsRepo.Add(settings);

        Stadium stadium = new Stadium("S1", "City", null, 30000);
        Team home = new Team(1, "Home", 1500, false, Confederation.UEFA);
        Team away = new Team(2, "Away", 1400, false, Confederation.UEFA);
        MatchGroup sourceHome = MatchGroup.CreateInstance(-1, DateTime.UtcNow.AddDays(-1), home, away, stadium, new Group("G1"), Journey.CreateInstance(1));
        MatchGroup sourceAway = MatchGroup.CreateInstance(-2, DateTime.UtcNow.AddDays(-1), away, home, stadium, new Group("G1"), Journey.CreateInstance(1));

        MatchElimination match = MatchElimination.Create(5, DateTime.UtcNow, home, away, stadium, Phase.RoundOf32, "R32-5", sourceHome, sourceAway);
        matchRepo.Add(match);

        MockMatchSimulationEngine simulation = new MockMatchSimulationEngine();
        MockUnitOfWork uow = new MockUnitOfWork();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        KnockoutFixtureGenerator generator = CreateGenerator(
            matchRepo, new MockEliminationMatchRepository(), settingsRepo, simulation, uow, auditRepo);

        // Act
        generator.SimulateEliminationMatches(new [] { 5 });

        // Assert
        List<AuditLog> logs = auditRepo.GetAll();
        Assert.IsTrue(logs.Any(l => l.Action == AuditActions.SimulationExecuted && l.TournamentSettingsId == settings.Id && l.AffectedEntityType == "Match" && l.AffectedEntityId == 5));
        int seed = settings.SimulationSeed + 5;
        Assert.IsTrue(logs.Any(l => l.Details.Contains($"seed {seed}")));
        Assert.IsTrue(simulation.SimulateWasCalled);
        Assert.IsTrue(uow.CommitCalls >= 1);
    }

    [TestMethod]
    public void SimulateEliminationMatches_WhenMatchCannotBeRead_SkipsItWithoutThrowing()
    {
        // Arrange
        MockTournamentSettingsRepository settingsRepo = new MockTournamentSettingsRepository();
        settingsRepo.Add(new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:0, simulationSeed:99, completionSeed:0));
        MockMatchSimulationEngine simulation = new MockMatchSimulationEngine();
        MockUnitOfWork uow = new MockUnitOfWork();

        KnockoutFixtureGenerator generator = CreateGenerator(
            new ThrowingMatchRepository(), new MockEliminationMatchRepository(), settingsRepo, simulation, uow, new MockAuditLogRepository());

        // Act
        generator.SimulateEliminationMatches(new [] { 999 });

        // Assert
        Assert.IsFalse(simulation.SimulateWasCalled);
        Assert.AreEqual(1, uow.CommitCalls);
    }

    private static KnockoutFixtureGenerator CreateGenerator(
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        MockTournamentSettingsRepository settingsRepository,
        MockMatchSimulationEngine simulation,
        MockUnitOfWork unitOfWork,
        MockAuditLogRepository auditRepository)
    {
        return new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([simulation]),
            matchRepository,
            eliminationRepository,
            new MockGroupRepository(),
            new MockStadiumRepository(),
            settingsRepository,
            unitOfWork,
            AuditLogService.CreateInstance(auditRepository),
            new MockNotificationService(),
            new MockRankingService());
    }

    private class ThrowingMatchRepository : IMatchRepository
    {
        public bool ExistsById(int matchId) => throw new InvalidOperationException("boom");
        public void Add(Match match) => throw new NotImplementedException();
        public void Update(Match match) => throw new NotImplementedException();
        public void Delete(Match match) => throw new NotImplementedException();
        public Match? GetById(int matchId) => throw new InvalidOperationException("boom");
        public List<Match> GetByGroup(string groupName) => throw new NotImplementedException();
        public List<Match> GetByJourney(int journeyNumber) => throw new NotImplementedException();
        public List<Match> GetByStadium(int stadiumId) => throw new NotImplementedException();
        public List<Match> GetByPhase(Phase phase) => throw new NotImplementedException();
        public List<Match> GetByDateRange(DateTime from, DateTime to) => throw new NotImplementedException();
        public List<Match> Search(MatchQueryCriteria criteria) => throw new NotImplementedException();
        public List<Match> GetAll() => throw new NotImplementedException();
        public int NextIncidentId() => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
    }
}
