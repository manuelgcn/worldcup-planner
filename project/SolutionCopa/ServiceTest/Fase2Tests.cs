using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using DataAccess.Interfaces;
using Services.Interfaces;

namespace ServiceTest
{
    [TestClass]
    public class Fase2Tests
    {
        [TestMethod]
        public void SimulateEliminationMatches_CallsSimulationAndCommits()
        {
           
            var matchId = 20;
            var team1 = new Team(1, "TeamA", 1200, false, Confederation.UEFA);
            var team2 = new Team(2, "TeamB", 1100, false, Confederation.UEFA);
            var stadium = new Stadium("S1", "City", null, 30000);

            var source1 = MatchGroup.CreateInstance(10, DateTime.Now.AddDays(-1), team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
            var source2 = MatchGroup.CreateInstance(11, DateTime.Now.AddDays(-1), team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

            var elimMatch = MatchElimination.Create(matchId, DateTime.Now.AddDays(1), team1, team2, stadium, Phase.RoundOf16, "R16-1", source1, source2);

            // Use existing test doubles
            var repo = new ServiceTest.TestDoubles.Mocks.MockMatchRepository();
            repo.Add(elimMatch);

            var settingsRepo = new DataAccess.Repositories.TournamentSettingsRepository();
            var ts = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, 0, 0, 42, 0);
            settingsRepo.Add(ts);

            var simulation = new ServiceTest.TestDoubles.Mocks.MockMatchSimulationEngine();
            var uow = new ServiceTest.TestDoubles.Mocks.MockUnitOfWork();

            var auditRepo = new ServiceTest.TestDoubles.Mocks.MockAuditLogRepository();
            var auditService = AuditLogService.CreateInstance(auditRepo, defaultTournamentSettingsId: 99);

            var fase2 = new Fase2(simulation, repo, settingsRepo, uow, auditService);

            // Act
            fase2.SimulateEliminationMatches(new[] { matchId });

            // Assert
            Assert.IsTrue(simulation.SimulateWasCalled, "Simulation should be called");
            Assert.AreEqual(42 + matchId, simulation.LastSeedReceived);
            Assert.IsNotNull(simulation.LastMatchReceived);
            Assert.AreEqual(matchId, simulation.LastMatchReceived!.MatchId);
            Assert.AreEqual(1, repo.UpdateCalls, "Repository Update should be called once");
            Assert.AreEqual(1, uow.CommitCalls, "Commit should be called once");
            var logs = auditRepo.GetAll();
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(matchId, logs[0].TournamentSettingsId);
        }

        
    }
}
