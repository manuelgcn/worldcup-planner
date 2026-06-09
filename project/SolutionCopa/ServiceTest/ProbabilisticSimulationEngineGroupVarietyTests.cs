using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public sealed class ProbabilisticSimulationEngineGroupVarietyTests
{
    private const int GroupCount = 12;
    private const int TeamsPerGroup = 4;
    private const int FixedSimulationSeed = 42;

    private ProbabilisticSimulationEngine _engine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _engine = new ProbabilisticSimulationEngine();
    }

    [TestMethod]
    public void Simulate_GroupStage_DoesNotCollapseToSingleScore()
    {
        List<Match> matches = BuildGroupStageMatches();

        foreach (Match match in matches)
        {
            _engine.Simulate(match, FixedSimulationSeed);
        }

        HashSet<string> distinctScores = matches
            .Select(match => $"{match.HomeGoals}-{match.AwayGoals}")
            .ToHashSet();

        Assert.IsTrue(
            distinctScores.Count >= 5,
            $"Expected at least 5 distinct scores across 72 group matches, got {distinctScores.Count}.");
    }

    [TestMethod]
    public void Simulate_GroupStage_TwoZeroScoresAreNotTheMajority()
    {
        List<Match> matches = BuildGroupStageMatches();

        foreach (Match match in matches)
        {
            _engine.Simulate(match, FixedSimulationSeed);
        }

        int twoZeroCount = matches.Count(m => m.HomeGoals == 2 && m.AwayGoals == 0);
        double ratio = (double)twoZeroCount / matches.Count;

        Assert.IsTrue(
            ratio < 0.4,
            $"More than 40% of matches ended 2-0 ({twoZeroCount}/{matches.Count}). Simulation is biased.");
    }

    [TestMethod]
    public void Simulate_GroupStage_IsDeterministicAcrossRuns()
    {
        List<Match> firstRun = BuildGroupStageMatches();
        List<Match> secondRun = BuildGroupStageMatches();

        foreach (Match match in firstRun)
        {
            _engine.Simulate(match, FixedSimulationSeed);
        }
        foreach (Match match in secondRun)
        {
            _engine.Simulate(match, FixedSimulationSeed);
        }

        for (int i = 0; i < firstRun.Count; i++)
        {
            Assert.AreEqual(firstRun[i].HomeGoals, secondRun[i].HomeGoals,
                $"Match {i} home goals differ across runs with the same seed.");
            Assert.AreEqual(firstRun[i].AwayGoals, secondRun[i].AwayGoals,
                $"Match {i} away goals differ across runs with the same seed.");
        }
    }

    private static List<Match> BuildGroupStageMatches()
    {
        List<Match> matches = new List<Match>();
        int teamId = 1;
        int matchId = 1;

        for (int groupIndex = 0; groupIndex < GroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);

            List<Team> teams = new List<Team>();
            for (int t = 0; t < TeamsPerGroup; t++)
            {
                Team team = new Team(teamId, $"Team {teamId}", 2400 - (teamId * 5), false, Confederation.UEFA);
                teams.Add(team);
                group.AddTeam(team);
                teamId++;
            }

            Stadium stadium = new Stadium($"Stadium {groupName}", "City", null, 30000);
            DateTime matchDate = new DateTime(2026, 6, 1);

            (int home, int away)[] pairings = new[]
            {
                (0, 3), (1, 2),
                (0, 2), (1, 3),
                (0, 1), (2, 3)
            };

            foreach ((int home, int away) in pairings)
            {
                MatchGroup match = MatchGroup.CreateInstance(
                    matchId,
                    matchDate,
                    teams[home],
                    teams[away],
                    stadium,
                    group,
                    Journey.CreateInstance(1));
                match.SetMatchCode($"M{matchId}");
                matches.Add(match);
                matchId++;
            }
        }

        return matches;
    }
}
