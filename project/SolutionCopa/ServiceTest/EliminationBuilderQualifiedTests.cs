using ProjectCopa;
using ProjectCopa.Models;
using Services;

namespace ServiceTest;

// H2: el R32 debe respetar las categorias A1..A8 / B1..B8 de CLAUDE.md §7 a partir de
// QualifiedTeams, sin perder la posicion (1ro/2do/3ro) al aplanar a (Team, Group).
[TestClass]
public class EliminationBuilderQualifiedTests
{
    private const int CrossingsSeed = 123;

    private static readonly TournamentSettings Settings =
        new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, fixtureSeed: 0, crossingsSeed: CrossingsSeed, simulationSeed: 0, completionSeed: 0);

    [TestMethod]
    public void CreateRoundOf32FromQualified_Produces16Matches()
    {
        List<MatchElimination> matches = BuildR32(BuildQualified());

        Assert.AreEqual(16, matches.Count);
        Assert.IsTrue(matches.All(match => match.Phase == Phase.RoundOf32));
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_LabelsEightAMatchesAndEightBMatches()
    {
        List<MatchElimination> matches = BuildR32(BuildQualified());

        List<string> codes = matches.Select(match => match.MatchCode!).ToList();
        for (int i = 1; i <= 8; i++)
        {
            Assert.IsTrue(codes.Contains($"A{i}"), $"Falta el cruce A{i}.");
            Assert.IsTrue(codes.Contains($"B{i}"), $"Falta el cruce B{i}.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_AMatchesPairFirstsAgainstThirds()
    {
        QualifiedTeams qualified = BuildQualified();
        HashSet<int> firsts = qualified.TopEightFirsts.Select(t => t.Id).ToHashSet();
        HashSet<int> thirds = qualified.BestEightThirds.Select(t => t.Id).ToHashSet();

        foreach (MatchElimination match in BuildR32(qualified).Where(m => m.MatchCode!.StartsWith("A")))
        {
            bool firstVsThird =
                (firsts.Contains(match.HomeTeam!.Id) && thirds.Contains(match.AwayTeam!.Id)) ||
                (firsts.Contains(match.AwayTeam!.Id) && thirds.Contains(match.HomeTeam!.Id));
            Assert.IsTrue(firstVsThird, $"{match.MatchCode} debe enfrentar un 1ro vs un mejor 3ro.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_B1ToB4PairRemainingFirstsAgainstWorstSeconds()
    {
        QualifiedTeams qualified = BuildQualified();
        HashSet<int> remainingFirsts = qualified.RemainingFourFirsts.Select(t => t.Id).ToHashSet();
        HashSet<int> worstSeconds = qualified.WorstFourSeconds.Select(t => t.Id).ToHashSet();

        List<MatchElimination> matches = BuildR32(qualified);
        foreach (string code in new[] { "B1", "B2", "B3", "B4" })
        {
            MatchElimination match = matches.First(m => m.MatchCode == code);
            bool ok =
                (remainingFirsts.Contains(match.HomeTeam!.Id) && worstSeconds.Contains(match.AwayTeam!.Id)) ||
                (remainingFirsts.Contains(match.AwayTeam!.Id) && worstSeconds.Contains(match.HomeTeam!.Id));
            Assert.IsTrue(ok, $"{code} debe enfrentar un primero restante vs un segundo peor.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_B5ToB8PairTopSecondsAmongThemselves()
    {
        QualifiedTeams qualified = BuildQualified();
        HashSet<int> topSeconds = qualified.TopEightSeconds.Select(t => t.Id).ToHashSet();

        List<MatchElimination> matches = BuildR32(qualified);
        foreach (string code in new[] { "B5", "B6", "B7", "B8" })
        {
            MatchElimination match = matches.First(m => m.MatchCode == code);
            Assert.IsTrue(
                topSeconds.Contains(match.HomeTeam!.Id) && topSeconds.Contains(match.AwayTeam!.Id),
                $"{code} debe enfrentar dos mejores segundos entre si.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_NeverPairsTwoFirsts()
    {
        QualifiedTeams qualified = BuildQualified();
        HashSet<int> allFirsts = qualified.TopEightFirsts.Concat(qualified.RemainingFourFirsts).Select(t => t.Id).ToHashSet();

        foreach (MatchElimination match in BuildR32(qualified))
        {
            bool bothFirsts = allFirsts.Contains(match.HomeTeam!.Id) && allFirsts.Contains(match.AwayTeam!.Id);
            Assert.IsFalse(bothFirsts, $"{match.MatchCode} enfrenta dos primeros.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_AvoidsSameGroupPairings()
    {
        QualifiedTeams qualified = BuildQualified();

        foreach (MatchElimination match in BuildR32(qualified))
        {
            string homeGroup = qualified.GroupByTeamId[match.HomeTeam!.Id];
            string awayGroup = qualified.GroupByTeamId[match.AwayTeam!.Id];
            Assert.AreNotEqual(homeGroup, awayGroup, $"{match.MatchCode} empareja mismo grupo {homeGroup}.");
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_OrdersMatchesSoWinnersPairAiVsBi()
    {
        // El bracket post-32 (C1..C8 = ganadores Ai vs Bi) empareja adyacentes:
        // la salida debe quedar intercalada A1,B1,A2,B2,...,A8,B8.
        List<MatchElimination> matches = BuildR32(BuildQualified());

        for (int i = 0; i < 8; i++)
        {
            Assert.AreEqual($"A{i + 1}", matches[2 * i].MatchCode);
            Assert.AreEqual($"B{i + 1}", matches[2 * i + 1].MatchCode);
        }
    }

    [TestMethod]
    public void CreateRoundOf32FromQualified_IsDeterministicForSameSeed()
    {
        List<string> first = BuildR32(BuildQualified()).Select(Signature).ToList();
        List<string> second = BuildR32(BuildQualified()).Select(Signature).ToList();

        CollectionAssert.AreEqual(first, second);
    }

    private static string Signature(MatchElimination match) =>
        $"{match.MatchCode}:{Math.Min(match.HomeTeam!.Id, match.AwayTeam!.Id)}-{Math.Max(match.HomeTeam!.Id, match.AwayTeam!.Id)}";

    private static List<MatchElimination> BuildR32(QualifiedTeams qualified)
    {
        EliminationBuilder builder = new EliminationBuilder();
        List<Stadium> stadiums = new List<Stadium>
        {
            new Stadium("S1", "City", null, 30000) { Id = 1 },
            new Stadium("S2", "City", null, 35000) { Id = 2 }
        };
        Func<int, Stadium> stadiumProvider = i => stadiums[i % stadiums.Count];
        Func<int, DateTime> dateProvider = i => Settings.StartDate.AddDays(i);

        return builder.CreateRoundOf32(qualified, stadiums, Settings, CrossingsSeed, stadiumProvider, dateProvider);
    }

    // 12 grupos A..L, 4 equipos cada uno (ids 1..48). Firsts/seconds/thirds salen del mismo
    // conjunto de grupos para forzar al algoritmo a desarmar los mismos-grupo (derangement).
    private static QualifiedTeams BuildQualified()
    {
        List<Group> groups = new List<Group>();
        int teamId = 1;
        for (char name = 'A'; name <= 'L'; name++)
        {
            Group group = new Group(name.ToString());
            for (int i = 0; i < 4; i++)
            {
                group.AddTeam(new Team(teamId, $"T{teamId}", 2000 - teamId, false, Confederation.UEFA));
                teamId++;
            }
            groups.Add(group);
        }

        Dictionary<int, string> groupByTeamId = groups
            .SelectMany(group => group.Teams.Select(team => (team.Id, group.Name)))
            .ToDictionary(entry => entry.Id, entry => entry.Name);

        List<Team> firsts = groups.Select(group => group.Teams.ElementAt(0)).ToList();
        List<Team> seconds = groups.Select(group => group.Teams.ElementAt(1)).ToList();
        List<Team> thirds = groups.Take(8).Select(group => group.Teams.ElementAt(2)).ToList();

        return new QualifiedTeams
        {
            TopEightFirsts = firsts.Take(8).ToList(),
            RemainingFourFirsts = firsts.Skip(8).Take(4).ToList(),
            TopEightSeconds = seconds.Take(8).ToList(),
            WorstFourSeconds = seconds.Skip(8).Take(4).ToList(),
            BestEightThirds = thirds,
            GroupByTeamId = groupByTeamId
        };
    }
}
