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
    public void Simulate_UpdatesMatchScore()
    {
        // Arrange
        bool scoreChanged = false;

        // Act
        for (int seed = 1; seed <= 200; seed++)
        {
            Match match = CreateMatch(2200, 400);
            _simulationEngine.Simulate(match, seed);

            if (match.HomeGoals > 0 || match.AwayGoals > 0)
            {
                scoreChanged = true;
                break;
            }
        }

        // Assert
        Assert.IsTrue(scoreChanged);
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

    private static Match CreateMatch(int homeRanking, int awayRanking)
    {
        Team homeTeam = new Team("Home Team", homeRanking, false, Confederation.CONCACAF);
        Team awayTeam = new Team("Away Team", awayRanking, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        Group group = new Group("A");
        Journey journey = new Journey(1);

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
