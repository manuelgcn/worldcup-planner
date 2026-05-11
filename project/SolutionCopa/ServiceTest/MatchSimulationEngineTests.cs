using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public sealed class MatchSimulationEngineTests
{
    private MatchSimulationEngine _simulationEngine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _simulationEngine = new MatchSimulationEngine();
    }

    [TestMethod]
    public void Simulate_WithSameSeed_GeneratesSameResult()
    {
        // Arrange
        Match firstMatch = CreateMatch(1500, 1600);
        Match secondMatch = CreateMatch(1500, 1600);

        // Act
        _simulationEngine.Simulate(firstMatch, 123);
        _simulationEngine.Simulate(secondMatch, 123);

        // Assert
        Assert.AreEqual(firstMatch.HomeGoals, secondMatch.HomeGoals);
        Assert.AreEqual(firstMatch.AwayGoals, secondMatch.AwayGoals);
    }

    [TestMethod]
    public void Simulate_DoesNotGenerateNegativeGoals()
    {
        // Arrange
        Match match = CreateMatch(1500, 1600);

        // Act
        _simulationEngine.Simulate(match, 123);

        // Assert
        Assert.IsTrue(match.HomeGoals >= 0);
        Assert.IsTrue(match.AwayGoals >= 0);
    }

    [TestMethod]
    public void Simulate_GeneratesMultiplePossibleScores()
    {
        // Arrange
        HashSet<string> generatedScores = [];

        // Act
        for (int seed = 1; seed <= 100; seed++)
        {
            Match match = CreateMatch(1500, 1500);

            _simulationEngine.Simulate(match, seed);

            generatedScores.Add($"{match.HomeGoals}-{match.AwayGoals}");
        }

        // Assert
        Assert.IsTrue(generatedScores.Count >= 5);
    }

    [TestMethod]
    public void Simulate_WithNullMatch_ThrowsArgumentNullException()
    {
        // Arrange

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _simulationEngine.Simulate(null!, 123));
    }

    [TestMethod]
    public void Simulate_DoesNotGenerateAbsurdGoalCounts()
    {
        // Arrange
        Match match = CreateMatch(2200, 400);

        // Act
        _simulationEngine.Simulate(match, 123);

        // Assert
        Assert.IsTrue(match.HomeGoals <= 15);
        Assert.IsTrue(match.AwayGoals <= 15);
    }

    [TestMethod]
    public void Simulate_StrongerTeamWinsMoreOften()
    {
        // Arrange
        int strongTeamWins = 0;
        int weakTeamWins = 0;

        // Act
        for (int seed = 1; seed <= 200; seed++)
        {
            Match strongTeamMatch = CreateMatch(2200, 400);
            _simulationEngine.Simulate(strongTeamMatch, seed);

            if (strongTeamMatch.HasHomeTeamWon())
            {
                strongTeamWins++;
            }

            Match weakTeamMatch = CreateMatch(400, 2200);
            _simulationEngine.Simulate(weakTeamMatch, seed);

            if (weakTeamMatch.HasHomeTeamWon())
            {
                weakTeamWins++;
            }
        }

        // Assert
        Assert.IsTrue(strongTeamWins > weakTeamWins);
    }
    
    [TestMethod]
    public void Simulate_WithVeryWeakTeam_UsesMinimumScoringChance()
    {
        // Arrange
        Match match = CreateMatch(300, 2500);

        // Act
        _simulationEngine.Simulate(match, 123);

        // Assert
        Assert.IsTrue(match.HomeGoals >= 0);
        Assert.IsTrue(match.AwayGoals >= 0);
    }

    [TestMethod]
    public void Simulate_WithEqualRankings_GeneratesValidResult()
    {
        // Arrange
        Match match = CreateMatch(1500, 1500);

        // Act
        _simulationEngine.Simulate(match, 456);

        // Assert
        Assert.IsTrue(match.HomeGoals >= 0);
        Assert.IsTrue(match.AwayGoals >= 0);
    }

    [TestMethod]
    public void Simulate_WithExtremeRankingDifference_FavorsStrongTeam()
    {
        // Arrange
        int strongTeamVictories = 0;

        // Act
        for (int seed = 1; seed <= 500; seed++)
        {
            Match match = CreateMatch(2500, 300);

            _simulationEngine.Simulate(match, seed);

            if (match.HasHomeTeamWon())
            {
                strongTeamVictories++;
            }
        }

        // Assert
        Assert.IsTrue(strongTeamVictories > 300);
    }

    [TestMethod]
    public void Simulate_WithVeryStrongAwayTeam_FavorsAwayTeam()
    {
        // Arrange
        int awayTeamVictories = 0;

        // Act
        for (int seed = 1; seed <= 500; seed++)
        {
            Match match = CreateMatch(300, 2500);

            _simulationEngine.Simulate(match, seed);

            if (match.HasAwayTeamWon())
            {
                awayTeamVictories++;
            }
        }

        // Assert
        Assert.IsTrue(awayTeamVictories > 300);
    }

    [TestMethod]
    public void Simulate_NeverExceedsMaximumGoals()
    {
        // Arrange
        Match match = CreateMatch(2500, 300);

        // Act
        _simulationEngine.Simulate(match, 999);

        // Assert
        Assert.IsTrue(match.HomeGoals <= 15);
        Assert.IsTrue(match.AwayGoals <= 15);
    }

    private static Match CreateMatch(int homeRanking, int awayRanking)
    {
        Team homeTeam = new Team(1, "Home Team", homeRanking, false, Confederation.CONCACAF);
        Team awayTeam = new Team(2, "Away Team", awayRanking, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        Group group = new Group(1, "A");
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
}
