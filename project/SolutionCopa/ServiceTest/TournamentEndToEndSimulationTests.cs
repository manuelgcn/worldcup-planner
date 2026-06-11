using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public sealed class TournamentEndToEndSimulationTests
{
    private const int FixedSimulationSeed = 1337;
    private const int GroupCount = 12;
    private const int TeamsPerGroup = 4;
    private const int FirstKnockoutMatchId = 2000;

    private ProbabilisticSimulationEngine _engine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _engine = new ProbabilisticSimulationEngine();
    }

    [TestMethod]
    public void FullTournament_HasVariedGroupScoresAndProducesSingleChampion()
    {
        List<MatchGroup> groupMatches = BuildAndSimulateGroupStage();

        HashSet<string> distinctScores = groupMatches
            .Select(match => $"{match.HomeGoals}-{match.AwayGoals}")
            .ToHashSet();
        Assert.IsTrue(distinctScores.Count >= 5, $"Group stage should show variety in scores. Distinct: {distinctScores.Count}.");

        List<Team> qualifiedTeams = SelectQualifiedTeamsNaively(groupMatches);
        Assert.AreEqual(32, qualifiedTeams.Count);

        List<MatchElimination> roundOf32 = SimulateRound(qualifiedTeams, Phase.RoundOf32, FirstKnockoutMatchId);
        AssertEveryMatchHasWinner(roundOf32);

        List<MatchElimination> roundOf16 = SimulateRoundFromWinners(roundOf32, Phase.RoundOf16);
        AssertEveryMatchHasWinner(roundOf16);

        List<MatchElimination> quarterFinals = SimulateRoundFromWinners(roundOf16, Phase.QuarterFinals);
        AssertEveryMatchHasWinner(quarterFinals);

        List<MatchElimination> semiFinals = SimulateRoundFromWinners(quarterFinals, Phase.SemiFinal);
        AssertEveryMatchHasWinner(semiFinals);

        List<MatchElimination> finals = SimulateRoundFromWinners(semiFinals, Phase.Final);
        AssertEveryMatchHasWinner(finals);

        Assert.AreEqual(1, finals.Count, "Final should be a single match.");
        Team? champion = finals[0].GetWinner();
        Assert.IsNotNull(champion, "Tournament must produce a single champion.");

        List<MatchElimination> thirdPlace = SimulateThirdPlace(semiFinals);
        AssertEveryMatchHasWinner(thirdPlace);
    }

    [TestMethod]
    public void FullTournament_IsFullyDeterministicAcrossRuns()
    {
        (Team championOne, int homeOne, int awayOne) = RunTournamentAndCaptureFinal();
        (Team championTwo, int homeTwo, int awayTwo) = RunTournamentAndCaptureFinal();

        Assert.AreEqual(championOne.Id, championTwo.Id);
        Assert.AreEqual(homeOne, homeTwo);
        Assert.AreEqual(awayOne, awayTwo);
    }

    private (Team champion, int homeGoals, int awayGoals) RunTournamentAndCaptureFinal()
    {
        List<MatchGroup> groupMatches = BuildAndSimulateGroupStage();
        List<Team> qualified = SelectQualifiedTeamsNaively(groupMatches);

        List<MatchElimination> current = SimulateRound(qualified, Phase.RoundOf32, FirstKnockoutMatchId);
        current = SimulateRoundFromWinners(current, Phase.RoundOf16);
        current = SimulateRoundFromWinners(current, Phase.QuarterFinals);
        current = SimulateRoundFromWinners(current, Phase.SemiFinal);
        current = SimulateRoundFromWinners(current, Phase.Final);

        MatchElimination finalMatch = current[0];
        return (finalMatch.GetWinner()!, finalMatch.HomeGoals, finalMatch.AwayGoals);
    }

    private List<MatchGroup> BuildAndSimulateGroupStage()
    {
        List<MatchGroup> matches = BuildGroupStageMatches();
        foreach (MatchGroup match in matches)
        {
            _engine.Simulate(match, FixedSimulationSeed);
        }
        return matches;
    }

    private List<MatchElimination> SimulateRound(List<Team> teams, Phase phase, int firstId)
    {
        List<MatchElimination> matches = new List<MatchElimination>();
        int nextId = firstId;

        for (int i = 0; i < teams.Count; i += 2)
        {
            matches.Add(CreateAndSimulate(teams[i], teams[i + 1], phase, nextId));
            nextId++;
        }

        return matches;
    }

    private List<MatchElimination> SimulateRoundFromWinners(List<MatchElimination> previous, Phase phase)
    {
        List<MatchElimination> matches = new List<MatchElimination>();
        int nextId = previous.Last().MatchId + 1;

        for (int i = 0; i < previous.Count; i += 2)
        {
            Team home = previous[i].GetWinner() ?? previous[i].HomeTeam!;
            Team away = previous[i + 1].GetWinner() ?? previous[i + 1].HomeTeam!;
            matches.Add(CreateAndSimulate(home, away, phase, nextId));
            nextId++;
        }

        return matches;
    }

    private List<MatchElimination> SimulateThirdPlace(List<MatchElimination> semiFinals)
    {
        List<MatchElimination> matches = new List<MatchElimination>();
        Team firstLoser = LoserOf(semiFinals[0]);
        Team secondLoser = LoserOf(semiFinals[1]);

        matches.Add(CreateAndSimulate(firstLoser, secondLoser, Phase.ThirdPlace, 9000));
        return matches;
    }

    private static Team LoserOf(MatchElimination match)
    {
        Team winner = match.GetWinner()!;
        return ReferenceEquals(winner, match.HomeTeam) ? match.AwayTeam! : match.HomeTeam!;
    }

    private MatchElimination CreateAndSimulate(Team home, Team away, Phase phase, int matchId)
    {
        Stadium stadium = new Stadium($"K-Stadium{matchId}", "City", null, 40000);
        DateTime date = new DateTime(2026, 7, 5);
        TournamentSettings settings = new TournamentSettings();

        MatchGroup sourceHome = MatchGroup.CreateMatchWithConfig(
            matchId * 10 + 1, date,
            new Team(matchId * 100 + 1, "Src1", 1000, true, Confederation.AFC),
            new Team(matchId * 100 + 2, "Src2", 900, true, Confederation.AFC),
            stadium, settings, new Group("X"), Journey.CreateInstance(1));

        MatchGroup sourceAway = MatchGroup.CreateMatchWithConfig(
            matchId * 10 + 2, date,
            new Team(matchId * 100 + 3, "Src3", 1000, true, Confederation.AFC),
            new Team(matchId * 100 + 4, "Src4", 900, true, Confederation.AFC),
            stadium, settings, new Group("Y"), Journey.CreateInstance(1));

        MatchElimination match = MatchElimination.CreateMatchWithConfig(
            matchId, date, home, away, stadium, settings, phase, $"{phase}-{matchId}", sourceHome, sourceAway);

        _engine.Simulate(match, FixedSimulationSeed);
        return match;
    }

    private static void AssertEveryMatchHasWinner(IEnumerable<MatchElimination> matches)
    {
        foreach (MatchElimination match in matches)
        {
            Assert.IsNotNull(
                match.GetWinner(),
                $"Match {match.MatchId} ({match.Phase}) ended without a winner.");
        }
    }

    private static List<Team> SelectQualifiedTeamsNaively(List<MatchGroup> groupMatches)
    {
        return groupMatches
            .SelectMany(match => new[] { match.HomeTeam!, match.AwayTeam! })
            .GroupBy(team => team.Id)
            .Select(group => group.First())
            .Take(32)
            .ToList();
    }

    private static List<MatchGroup> BuildGroupStageMatches()
    {
        List<MatchGroup> matches = new List<MatchGroup>();
        int teamId = 1;
        int matchId = 1;

        for (int groupIndex = 0; groupIndex < GroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);

            List<Team> teams = new List<Team>();
            for (int t = 0; t < TeamsPerGroup; t++)
            {
                Team team = new Team(teamId, $"Team{teamId}", 2400 - (teamId * 5), false, Confederation.UEFA);
                teams.Add(team);
                group.AddTeam(team);
                teamId++;
            }

            Stadium stadium = new Stadium($"GStadium{groupName}", "City", null, 30000);
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
                    matchId, matchDate, teams[home], teams[away], stadium, group, Journey.CreateInstance(1));
                match.SetMatchCode($"M{matchId}");
                matches.Add(match);
                matchId++;
            }
        }

        return matches;
    }
}
