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
            // Arrange
            var matchId = 20;
            var team1 = new Team(1, "TeamA", 1200, false, Confederation.UEFA);
            var team2 = new Team(2, "TeamB", 1100, false, Confederation.UEFA);
            var stadium = new Stadium("S1", "City", null, 30000);

            var source1 = MatchGroup.CreateInstance(10, DateTime.Now.AddDays(-1), team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
            var source2 = MatchGroup.CreateInstance(11, DateTime.Now.AddDays(-1), team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

            var elimMatch = MatchElimination.Create(matchId, DateTime.Now.AddDays(1), team1, team2, stadium, Phase.RoundOf16, "R16-1", source1, source2);

            var repo = new FakeMatchRepository(elimMatch);
            var settingsRepo = new FakeSettingsRepository(new TournamentSettings(id:1, startDate: new DateTime(2026,6,1), maxMatchesPerDay:3, daysBetweenJourneys:3, fixtureSeed:0, crossingsSeed:0, simulationSeed:42, completionSeed:0));
            var simulation = new FakeSimulationEngine();
            var uow = new FakeUnitOfWork();
            var audit = new FakeAuditLogService();

            var fase2 = new Fase2(simulation, repo, settingsRepo, uow, audit);

            // Act
            fase2.SimulateEliminationMatches(new[] { matchId });

            // Assert
            Assert.AreEqual(1, simulation.Calls.Count, "Simulation should be called once");
            Assert.AreEqual(matchId, simulation.Calls[0].Match.MatchId);
            Assert.AreEqual(42 + matchId, simulation.Calls[0].Seed);
            Assert.IsTrue(repo.UpdatedCalled, "Repository Update should be called");
            Assert.AreEqual(1, uow.CommitCount, "Commit should be called once");
            Assert.IsTrue(audit.Logs.Count == 1, "Audit should have one entry");
        }

        // Fake implementations
        private class FakeMatchRepository : IMatchRepository
        {
            private readonly Match _match;
            public bool UpdatedCalled { get; private set; }

            public FakeMatchRepository(Match match)
            {
                _match = match;
            }

            public void Add(Match match) => throw new NotImplementedException();
            public void Delete(Match match) => throw new NotImplementedException();
            public IReadOnlyCollection<Match> GetAll() => new[] { _match };
            public Match? GetById(int id) => _match.MatchId == id ? _match : null;
            public IReadOnlyCollection<Match> GetByGroup(string groupName) => throw new NotImplementedException();
            public IReadOnlyCollection<Match> GetByJourney(int journeyNumber) => throw new NotImplementedException();
            public IReadOnlyCollection<Match> GetByPhase(Phase phase) => throw new NotImplementedException();
            public void Update(Match match) { UpdatedCalled = true; }
            public bool ExistsById(int id) => _match.MatchId == id;
        }

        private class FakeSettingsRepository : ITournamentSettingsRepository
        {
            private readonly TournamentSettings _settings;
            public FakeSettingsRepository(TournamentSettings settings) { _settings = settings; }
            public TournamentSettings? GetById(int id) => _settings;
            public void Add(TournamentSettings settings) => throw new NotImplementedException();
            public void Update(TournamentSettings settings) => throw new NotImplementedException();
        }

        private class FakeSimulationEngine : IMatchSimulationEngine
        {
            public List<(Match Match, int Seed)> Calls { get; } = new();
            public void Simulate(Match match, int simulationSeed)
            {
                Calls.Add((match, simulationSeed));
                // simple deterministic effect: set some goals
                match.SetGoals(1, 0);
            }
        }

        private class FakeUnitOfWork : IUnitOfWork
        {
            public int CommitCount { get; private set; }
            public void Commit() => CommitCount++;
        }

        private class FakeAuditLogService : IAuditLogService
        {
            public List<string> Logs { get; } = new();
            public void Log(string action, string details, int affectedEntityId, int tournamentSettingsId = 0, string? affectedEntityType = null)
            {
                Logs.Add($"{action}:{details}");
            }
        }
    }
}
