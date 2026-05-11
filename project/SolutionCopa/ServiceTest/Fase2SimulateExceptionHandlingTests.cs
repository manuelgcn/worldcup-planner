using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;
using System.Collections.Generic;

namespace ServiceTest
{
    [TestClass]
    public class Fase2SimulateExceptionHandlingTests
    {
        [TestMethod]
        public void SimulateEliminationMatches_Handles_GetById_Exception_And_Commits()
        {
            var simulation = new ServiceTest.TestDoubles.Mocks.MockMatchSimulationEngine();
            var throwingRepo = new ThrowingMatchRepository();
            var settingsRepo = new ServiceTest.TestDoubles.Mocks.MockTournamentSettingsRepository();
            settingsRepo.Add(new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, 0, 0, 10, 0));
            var uow = new ServiceTest.TestDoubles.Mocks.MockUnitOfWork();
            var auditRepo = new ServiceTest.TestDoubles.Mocks.MockAuditLogRepository();
            var auditService = Services.AuditLogService.CreateInstance(auditRepo, 1);

            var fase2 = new Fase2(simulation, throwingRepo, settingsRepo, uow, auditService);

            fase2.SimulateEliminationMatches(new [] { 999 });

            Assert.IsFalse(simulation.SimulateWasCalled, "Simulation should not be called when repo throws");
            Assert.AreEqual(1, uow.CommitCalls, "Commit should be called even when GetById throws");
        }

        private class ThrowingMatchRepository : DataAccess.Interfaces.IMatchRepository
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
