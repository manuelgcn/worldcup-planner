using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public class EliminationBuilderTests
{
    [TestMethod]
    public void CreateRoundOf32_NoSameGroupPairs_WhenPossible()
    {
        // Arrange
        EliminationBuilder builder = new EliminationBuilder();
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:123, simulationSeed:0, completionSeed:0);

        List<(Team Team, string Group)> entries = new List<(Team Team, string Group)>();
        int id = 1;
        for (int g = 1; g <= 16; g++)
        {
            entries.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            entries.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
        }

        List<Stadium> stadiums = new List<Stadium>
        {
            new Stadium("S1","City",null,30000),
            new Stadium("S2","City",null,35000)
        };

        Func<int, Stadium> stadiumProvider = i => stadiums[i % stadiums.Count];
        Func<int, DateTime> dateProvider = i => settings.StartDate.AddDays(i);

        // Act
        List<MatchElimination> matches = builder.CreateRoundOf32(entries, stadiums, settings, crossingsSeed: 123, stadiumProvider: stadiumProvider, dateProvider: dateProvider);

        // Assert
        // Map team id to group
        var teamGroup = entries.ToDictionary(e => e.Team.Id, e => e.Group);

        foreach (var m in matches)
        {
            int homeId = m.HomeTeam?.Id ?? -1;
            int awayId = m.AwayTeam?.Id ?? -1;
            string homeGroup = teamGroup[homeId];
            string awayGroup = teamGroup[awayId];
            Assert.AreNotEqual(homeGroup, awayGroup, $"Match {m.MatchId} paired same group {homeGroup}");
        }
    }

    [TestMethod]
    public void CreateRoundOf32_Produces_16_Matches()
    {
        EliminationBuilder builder = new EliminationBuilder();
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026,6,1), 3, 3, fixtureSeed:0, crossingsSeed:42, simulationSeed:0, completionSeed:0);

        List<(Team Team, string Group)> entries = new List<(Team Team, string Group)>();
        int id = 1;
        for (int g = 1; g <= 16; g++)
        {
            entries.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            entries.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
        }

        List<Stadium> stadiums = new List<Stadium> { new Stadium("S1","City",null,30000), new Stadium("S2","City",null,35000) };
        Func<int, Stadium> stadiumProvider = i => stadiums[i % stadiums.Count];
        Func<int, DateTime> dateProvider = i => settings.StartDate.AddDays(i);

        var matches = builder.CreateRoundOf32(entries, stadiums, settings, crossingsSeed: 42, stadiumProvider: stadiumProvider, dateProvider: dateProvider);
        Assert.AreEqual(16, matches.Count);
    }

    [TestMethod]
    public void CreateRoundOf32_ProducesInitialRoundWithoutSourceMatches()
    {
        EliminationBuilder builder = new EliminationBuilder();
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, fixtureSeed: 0, crossingsSeed: 7, simulationSeed: 0, completionSeed: 0);

        List<(Team Team, string Group)> entries = new List<(Team Team, string Group)>();
        int id = 1;
        for (int g = 1; g <= 16; g++)
        {
            entries.Add((new Team(id++, $"T{g}A", 1000 + id, false, Confederation.UEFA), $"G{g}"));
            entries.Add((new Team(id++, $"T{g}B", 1000 + id, false, Confederation.UEFA), $"G{g}"));
        }

        List<Stadium> stadiums = new List<Stadium> { new Stadium("S1", "City", null, 30000), new Stadium("S2", "City", null, 35000) };
        Func<int, Stadium> stadiumProvider = i => stadiums[i % stadiums.Count];
        Func<int, DateTime> dateProvider = i => settings.StartDate.AddDays(i);

        List<MatchElimination> matches = builder.CreateRoundOf32(entries, stadiums, settings, crossingsSeed: 7, stadiumProvider: stadiumProvider, dateProvider: dateProvider);

        // R32 es la ronda inicial: no fabrica partidos fuente sinteticos ni ids negativos.
        Assert.IsTrue(matches.All(m => m.MatchHome == null && m.MatchAway == null), "RoundOf32 must have no source matches.");
        Assert.IsTrue(matches.All(m => m.Phase == Phase.RoundOf32));
        Assert.IsTrue(matches.All(m => m.MatchId > 0), "RoundOf32 must not use negative/synthetic ids.");
    }
}