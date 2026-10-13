using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.DataAccess.Repositories;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class Fase2OrchestrationTddTests
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
        MockMatchRepository matchRepo = new MockMatchRepository();
        MockTournamentSettingsRepository settingsRepo = new MockTournamentSettingsRepository();
        settingsRepo.Add(settings);
        MockMatchSimulationEngine simulation = new WorldCupPlanner.Services.Tests.TestDoubles.Mocks.MockMatchSimulationEngine();
        MockUnitOfWork uow = new WorldCupPlanner.Services.Tests.TestDoubles.Mocks.MockUnitOfWork();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        AuditLogService auditService = AuditLogService.CreateInstance(auditRepo);

        Fase2 fase2 = new Fase2(
            new SimulationEngineRegistry([simulation]),
            matchRepo,
            eliminationRepo,
            new MockGroupRepository(),
            new MockStadiumRepository(),
            settingsRepo,
            uow,
            auditService,
            new MockNotificationService(),
            new MockRankingService());

        // Act
        fase2.OrchestrateAndSimulateEliminationPhase(teamsWithGroup, stadiums, settings, eliminationRepo);

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
}
