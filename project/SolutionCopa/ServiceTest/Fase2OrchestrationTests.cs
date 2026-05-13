using ProjectCopa;
using Services;
using DataAccess.Repositories;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class Fase2OrchestrationTests
{
    [TestMethod]
    public void Orchestrate_Creates_All_Rounds_And_Audits()
    {
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

        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:777, simulationSeed:0, completionSeed:0);

        EliminationMatchRepository elimRepo = new EliminationMatchRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        AuditLogService auditService = AuditLogService.CreateInstance(auditRepo, defaultTournamentSettingsId: settings.Id);

        EliminationOrchestrator orchestrator = new EliminationOrchestrator(elimRepo, auditService);

        orchestrator.OrchestrateEliminationPhase(teamsWithGroup, stadiums, settings);

        List<Match> all = elimRepo.GetAll().ToList();
        Assert.AreEqual(31, all.Count);

        Assert.AreEqual(16, all.Count(m => ((MatchElimination)m).Phase == Phase.RoundOf32));
        Assert.AreEqual(8, all.Count(m => ((MatchElimination)m).Phase == Phase.RoundOf16));
        Assert.AreEqual(4, all.Count(m => ((MatchElimination)m).Phase == Phase.QuarterFinals));
        Assert.AreEqual(2, all.Count(m => ((MatchElimination)m).Phase == Phase.SemiFinal));
        Assert.AreEqual(1, all.Count(m => ((MatchElimination)m).Phase == Phase.Final));

        List<AuditLog> logs = auditRepo.GetAll();
        Assert.IsTrue(logs.Any(l => l.Action == "GenerateEliminationBracket"));
    }
}