using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public sealed class PureRandomSimulationEngineTests
{
    private const string ExpectedEngineName = "Aleatorio Puro";
    private const int FixedSimulationSeed = 42;
    private const int MaxGoalsPerTeam = 5;
    private const int FirstEliminationMatchId = 2000;
    private const int LastEliminationMatchId = 2100;

    private PureRandomSimulationEngine _engine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _engine = new PureRandomSimulationEngine();
    }

    [TestMethod]
    public void Name_IsAleatorioPuro()
    {
        Assert.AreEqual(ExpectedEngineName, _engine.Name);
    }

    [TestMethod]
    public void Simulate_WithNullMatch_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _engine.Simulate(null!, FixedSimulationSeed));
    }

    [TestMethod]
    public void Simulate_WithSameSeed_GeneratesSameResult()
    {
        Match firstMatch = CreateMatch(1500, 1600);
        Match secondMatch = CreateMatch(1500, 1600);

        _engine.Simulate(firstMatch, FixedSimulationSeed);
        _engine.Simulate(secondMatch, FixedSimulationSeed);

        Assert.AreEqual(firstMatch.HomeGoals, secondMatch.HomeGoals);
        Assert.AreEqual(firstMatch.AwayGoals, secondMatch.AwayGoals);
    }

    [TestMethod]
    public void Simulate_IgnoresTeamRankings()
    {
        Match strongHomeMatch = CreateMatch(2500, 300);
        Match weakHomeMatch = CreateMatch(300, 2500);

        _engine.Simulate(strongHomeMatch, FixedSimulationSeed);
        _engine.Simulate(weakHomeMatch, FixedSimulationSeed);

        Assert.AreEqual(strongHomeMatch.HomeGoals, weakHomeMatch.HomeGoals);
        Assert.AreEqual(strongHomeMatch.AwayGoals, weakHomeMatch.AwayGoals);
    }

    [TestMethod]
    public void Simulate_KeepsGoalsBetweenZeroAndFive()
    {
        for (int seed = 1; seed <= 200; seed++)
        {
            Match match = CreateMatch(1500, 1500);

            _engine.Simulate(match, seed);

            Assert.IsTrue(match.HomeGoals >= 0 && match.HomeGoals <= MaxGoalsPerTeam,
                $"Home goals {match.HomeGoals} out of range for seed {seed}.");
            Assert.IsTrue(match.AwayGoals >= 0 && match.AwayGoals <= MaxGoalsPerTeam,
                $"Away goals {match.AwayGoals} out of range for seed {seed}.");
        }
    }

    [TestMethod]
    public void Simulate_GeneratesMultiplePossibleScores()
    {
        HashSet<string> generatedScores = [];

        for (int seed = 1; seed <= 100; seed++)
        {
            Match match = CreateMatch(1500, 1500);

            _engine.Simulate(match, seed);

            generatedScores.Add($"{match.HomeGoals}-{match.AwayGoals}");
        }

        Assert.IsTrue(generatedScores.Count >= 5);
    }

    [TestMethod]
    public void Simulate_MarksMatchAsSimulated()
    {
        Match match = CreateMatch(1500, 1500);

        _engine.Simulate(match, FixedSimulationSeed);

        Assert.IsTrue(match.WasSimulated);
    }

    [TestMethod]
    public void Simulate_EliminationMatch_NeverEndsWithoutWinner()
    {
        int tiedInRegulation = 0;

        for (int matchId = FirstEliminationMatchId; matchId <= LastEliminationMatchId; matchId++)
        {
            MatchElimination match = CreateMatchElimination(matchId);

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
            "Expected at least one match to be tied at full time so the 50/50 penalty shootout is exercised.");
    }

    [TestMethod]
    public void Simulate_EliminationMatch_IsDeterministicAcrossRuns()
    {
        MatchElimination first = CreateMatchElimination(FirstEliminationMatchId);
        MatchElimination second = CreateMatchElimination(FirstEliminationMatchId);

        _engine.Simulate(first, FixedSimulationSeed);
        _engine.Simulate(second, FixedSimulationSeed);

        Assert.AreEqual(first.HomeGoals, second.HomeGoals);
        Assert.AreEqual(first.AwayGoals, second.AwayGoals);
        Assert.AreEqual(first.GetWinner()?.Id, second.GetWinner()?.Id);
    }

    private static Match CreateMatch(int homeRanking, int awayRanking)
    {
        Team homeTeam = new Team(1, "Home Team", homeRanking, false, Confederation.CONCACAF);
        Team awayTeam = new Team(2, "Away Team", awayRanking, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);

        return MatchGroup.CreateInstance(
            1,
            new DateTime(2026, 6, 1),
            homeTeam,
            awayTeam,
            stadium,
            group,
            journey);
    }

    private static MatchElimination CreateMatchElimination(int matchId)
    {
        Team home = new Team(matchId + 1, $"Home{matchId}", 1500, false, Confederation.UEFA);
        Team away = new Team(matchId + 2, $"Away{matchId}", 1500, false, Confederation.CONMEBOL);
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
