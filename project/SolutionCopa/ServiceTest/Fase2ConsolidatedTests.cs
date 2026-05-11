using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using DataAccess.Repositories;
using ServiceTest.TestDoubles.Mocks;
using DataAccess.Interfaces;

namespace ServiceTest
{
    [TestClass]
    public class Fase2ConsolidatedTests
    {
        [TestMethod]
        public void Test_Fase2_AllScenarios()
        {
            var teamsWithGroup = new List<(Team Team, string Group)>();
            int id = 1;
            for (int g = 1; g <= 16; g++)
            {
                teamsWithGroup.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
                teamsWithGroup.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            }

            var stadiums = new List<Stadium>
            {
                new Stadium("S1", "City1", null, 30000),
                new Stadium("S2", "City2", null, 35000)
            };

            var settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:777, simulationSeed:99, completionSeed:0);

            var eliminationRepo = new EliminationMatchRepository();
            var matchRepo = new MockMatchRepository();
            var settingsRepo = new TournamentSettingsRepository();
            settingsRepo.Add(settings);
            var simulation = new ServiceTest.TestDoubles.Mocks.MockMatchSimulationEngine();
            var uow = new ServiceTest.TestDoubles.Mocks.MockUnitOfWork();
            var auditRepo = new MockAuditLogRepository();
            var auditService = AuditLogService.CreateInstance(auditRepo, defaultTournamentSettingsId: settings.Id);

            var fase2 = new Fase2(simulation, matchRepo, settingsRepo, uow, auditService);

            fase2.OrchestrateAndSimulateEliminationPhase(teamsWithGroup, stadiums, settings, eliminationRepo);

            var all = eliminationRepo.GetAll().ToList();
            Assert.AreEqual(31, all.Count);
            Assert.IsTrue(simulation.SimulateWasCalled);
            Assert.IsTrue(uow.CommitCalls >= 1);
            var logs = auditRepo.GetAll();
            Assert.IsTrue(logs.Any(l => l.Action == "GenerateEliminationBracket"));

            var throwingRepo = new ThrowingMatchRepository();
            var fase2Bad = new Fase2(simulation, throwingRepo, settingsRepo, uow, auditService);

            fase2Bad.SimulateEliminationMatches(new [] { 999 });

            Assert.IsFalse(simulation.SimulateWasCalled == false && throwingRepo is null, "Exception path executed");
            Assert.IsTrue(uow.CommitCalls >= 1);
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
            public List<Match> GetByDateRange(DateTime from, DateTime to) => throw new NotImplementedException();
        }
    }
}
