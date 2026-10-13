using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class ProbabilisticSimulationEngineEliminationTests
{
    private const int FixedSimulationSeed = 42;
    private const int FirstMatchId = 2000;
    private const int LastMatchId = 2100;

    private ProbabilisticSimulationEngine _engine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _engine = new ProbabilisticSimulationEngine();
    }

    [TestMethod]
    public void Simulate_EliminationMatch_NeverEndsWithoutWinner()
    {
        int tiedInRegulation = 0;

        for (int matchId = FirstMatchId; matchId <= LastMatchId; matchId++)
        {
            MatchElimination match = CreateMatchElimination(matchId, homeRanking: 1500, awayRanking: 1500);

            _engine.Simulate(match, FixedSimulationSeed);

            if (match.HomeGoals == match.AwayGoals)
            {
                tiedInRegulation++;
            }

            Assert.IsNotNull(
                match.GetWinner(),
                $"Elimination match {matchId} ended without a winner (goals {match.HomeGoals}-{match.AwayGoals}).");
        }

        Assert.IsTrue(
            tiedInRegulation > 0,
            "Expected at least one match to be tied at full time so the tiebreaker is exercised.");
    }

    [TestMethod]
    public void Simulate_EliminationMatch_IsDeterministicAcrossRuns()
    {
        MatchElimination first = CreateMatchElimination(FirstMatchId, homeRanking: 1500, awayRanking: 1500);
        MatchElimination second = CreateMatchElimination(FirstMatchId, homeRanking: 1500, awayRanking: 1500);

        _engine.Simulate(first, FixedSimulationSeed);
        _engine.Simulate(second, FixedSimulationSeed);

        Assert.AreEqual(first.HomeGoals, second.HomeGoals);
        Assert.AreEqual(first.AwayGoals, second.AwayGoals);
        Assert.AreEqual(first.GetWinner()?.Id, second.GetWinner()?.Id);
    }

    private static MatchElimination CreateMatchElimination(int matchId, int homeRanking, int awayRanking)
    {
        Team home = new Team(matchId + 1, $"Home{matchId}", homeRanking, false, Confederation.UEFA);
        Team away = new Team(matchId + 2, $"Away{matchId}", awayRanking, false, Confederation.CONMEBOL);
        Stadium stadium = new Stadium($"Stadium{matchId}", "City", null, 40000);
        DateTime date = new DateTime(2026, 7, 5);

        TournamentSettings settings = new TournamentSettings();

        Team sourceHomeA = new Team(matchId + 3, "SrcHomeA", 1000, true, Confederation.AFC);
        Team sourceHomeB = new Team(matchId + 4, "SrcHomeB", 900, true, Confederation.AFC);
        MatchGroup sourceHome = MatchGroup.CreateMatchWithConfig(
            matchId - 1000, date, sourceHomeA, sourceHomeB, stadium, settings, new Group("A"), Journey.CreateInstance(1));

        Team sourceAwayA = new Team(matchId + 5, "SrcAwayA", 1000, true, Confederation.AFC);
        Team sourceAwayB = new Team(matchId + 6, "SrcAwayB", 900, true, Confederation.AFC);
        MatchGroup sourceAway = MatchGroup.CreateMatchWithConfig(
            matchId - 500, date, sourceAwayA, sourceAwayB, stadium, settings, new Group("B"), Journey.CreateInstance(1));

        return MatchElimination.CreateMatchWithConfig(
            matchId, date, home, away, stadium, settings, Phase.RoundOf16, $"E-{matchId}", sourceHome, sourceAway);
    }
}
