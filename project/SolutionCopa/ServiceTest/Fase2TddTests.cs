using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;
using DataAccess.Repositories;

namespace ServiceTest;

[TestClass]
public class Fase2TddTests
{
    [TestMethod]
    public void SimulateEliminationMatches_LogsEachMatch_WithSeedAndTournamentId()
    {
        // Arrange
        var matchRepo = new MockMatchRepository();
        var settingsRepo = new TournamentSettingsRepository();
        var settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:0, simulationSeed:42, completionSeed:0);
        settingsRepo.Add(settings);

        var stadium = new Stadium("S1", "City", null, 30000);
        var home = new Team(1, "Home", 1500, false, Confederation.UEFA);
        var away = new Team(2, "Away", 1400, false, Confederation.UEFA);
        var sourceHome = MatchGroup.CreateInstance(-1, DateTime.UtcNow.AddDays(-1), home, away, stadium, new Group("G1"), Journey.CreateInstance(1));
        var sourceAway = MatchGroup.CreateInstance(-2, DateTime.UtcNow.AddDays(-1), away, home, stadium, new Group("G1"), Journey.CreateInstance(1));

        var match = MatchElimination.Create(5, DateTime.UtcNow, home, away, stadium, Phase.RoundOf32, "R32-5", sourceHome, sourceAway);
        matchRepo.Add(match);

        var simulation = new MockMatchSimulationEngine();
        var uow = new ServiceTest.TestDoubles.Mocks.MockUnitOfWork();
        var auditRepo = new MockAuditLogRepository();
        var auditService = AuditLogService.CreateInstance(auditRepo, defaultTournamentSettingsId: settings.Id);

        var fase2 = new Fase2(simulation, matchRepo, settingsRepo, uow, auditService);

        // Act
        fase2.SimulateEliminationMatches(new [] { 5 });

        // Assert
        var logs = auditRepo.GetAll();
        Assert.IsTrue(logs.Any(l => l.Action == AuditActions.SimulateMatch && l.TournamentSettingsId == settings.Id && l.AffectedEntityType == "Match" && l.AffectedEntityId == 5));
        var seed = settings.SimulationSeed + 5;
        Assert.IsTrue(logs.Any(l => l.Details.Contains($"seed {seed}")));
        Assert.IsTrue(simulation.SimulateWasCalled);
        Assert.IsTrue(uow.CommitCalls >= 1);
    }
}