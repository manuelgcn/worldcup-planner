using DataAccess.Interfaces;
using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class MatchRepositorySearchTests
{
    private InMemoryDbContextFactory _factory = null!;
    private MatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new InMemoryDbContextFactory();
        _repository = new MatchRepository(_factory);
    }

    [TestMethod]
    public void SearchWithoutCriteriaReturnsEveryStoredMatchOnce()
    {
        // El bracket inserta dos partidos de grupo (fuentes) + un partido de eliminacion.
        // Search recorre la jerarquia TPH una sola vez: la eliminatoria NO se duplica.
        AddBracket(eliminationId: 201, sourceIdBase: 100);

        List<Match> result = _repository.Search(new MatchQueryCriteria());

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result.OfType<MatchElimination>().Count());
    }

    [TestMethod]
    public void SearchByStadiumReturnsOnlyMatchesAtThatStadium()
    {
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        Stadium centenario = EfFixtureData.MakeStadium(1, "Centenario");
        Stadium maracana = EfFixtureData.MakeStadium(2, "Maracana");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, group, journey, centenario, maracana, teams[0], teams[1], teams[2], teams[3]);
        _repository.Add(EfFixtureData.MakeGroupMatch(1, Date(1), teams[0], teams[1], centenario, group, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(2, Date(2), teams[2], teams[3], maracana, group, journey));

        List<Match> result = _repository.Search(new MatchQueryCriteria { StadiumId = 1 });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].MatchId);
    }

    [TestMethod]
    public void SearchByGroupReturnsOnlyMatchesOfThatGroup()
    {
        Group groupA = new Group("A");
        Group groupB = new Group("B");
        Journey journey = Journey.CreateInstance(1);
        Stadium stadium = EfFixtureData.MakeStadium(1, "St1");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, groupA, groupB, journey, stadium, teams[0], teams[1], teams[2], teams[3]);
        _repository.Add(EfFixtureData.MakeGroupMatch(7, Date(5), teams[0], teams[1], stadium, groupA, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(8, Date(5), teams[2], teams[3], stadium, groupB, journey));

        List<Match> result = _repository.Search(new MatchQueryCriteria { GroupName = "A" });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(7, result[0].MatchId);
    }

    [TestMethod]
    public void SearchByPhaseReturnsOnlyEliminationMatchesOfThatPhase()
    {
        AddBracket(eliminationId: 10, phase: Phase.RoundOf16, sourceIdBase: 100, groupHomeName: "X", groupAwayName: "Y");
        AddBracket(eliminationId: 11, phase: Phase.QuarterFinals, sourceIdBase: 300, groupHomeName: "P", groupAwayName: "Q");

        List<Match> result = _repository.Search(new MatchQueryCriteria { Phase = Phase.RoundOf16 });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(10, result[0].MatchId);
    }

    [TestMethod]
    public void SearchByDateRangeIsInclusiveFromAndExclusiveTo()
    {
        AddIsolatedGroupMatch(matchId: 10, groupName: "A", date: Date(10));
        AddIsolatedGroupMatch(matchId: 20, groupName: "B", date: Date(20));
        AddIsolatedGroupMatch(matchId: 21, groupName: "C", date: Date(21));
        AddIsolatedGroupMatch(matchId: 25, groupName: "D", date: Date(25));

        List<Match> result = _repository.Search(new MatchQueryCriteria { From = Date(10), To = Date(21) });

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(match => match.MatchId == 10));
        Assert.IsTrue(result.Any(match => match.MatchId == 20));
    }

    [TestMethod]
    public void SearchCombinesFiltersWithAnd()
    {
        Group groupA = new Group("A");
        Group groupB = new Group("B");
        Journey journey = Journey.CreateInstance(1);
        Stadium stadium = EfFixtureData.MakeStadium(1, "St1");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, groupA, groupB, journey, stadium, teams[0], teams[1], teams[2], teams[3]);
        _repository.Add(EfFixtureData.MakeGroupMatch(1, Date(10), teams[0], teams[1], stadium, groupA, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(2, Date(10), teams[2], teams[3], stadium, groupB, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(3, Date(20), teams[0], teams[2], stadium, groupA, journey));

        List<Match> result = _repository.Search(new MatchQueryCriteria { GroupName = "A", From = Date(15), To = Date(26) });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(3, result[0].MatchId);
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

    private static Team[] FourTeams(int baseId) =>
    [
        EfFixtureData.MakeTeam(baseId, $"T{baseId}"),
        EfFixtureData.MakeTeam(baseId + 1, $"T{baseId + 1}"),
        EfFixtureData.MakeTeam(baseId + 2, $"T{baseId + 2}"),
        EfFixtureData.MakeTeam(baseId + 3, $"T{baseId + 3}"),
    ];

    private void AddIsolatedGroupMatch(int matchId, string groupName, DateTime date)
    {
        Team home = EfFixtureData.MakeTeam(matchId * 100 + 1, $"H{matchId}");
        Team away = EfFixtureData.MakeTeam(matchId * 100 + 2, $"A{matchId}");
        Stadium stadium = EfFixtureData.MakeStadium(matchId * 100 + 3, $"St{matchId}");
        Group group = new Group(groupName);
        Journey journey = Journey.CreateInstance(matchId);
        EfFixtureData.Seed(_factory, home, away, stadium, group, journey);
        _repository.Add(EfFixtureData.MakeGroupMatch(matchId, date, home, away, stadium, group, journey));
    }

    private void AddBracket(int eliminationId, Phase phase = Phase.RoundOf16,
        int sourceIdBase = 100, string groupHomeName = "X", string groupAwayName = "Y")
    {
        Team a = EfFixtureData.MakeTeam(sourceIdBase + 1, $"A{sourceIdBase}");
        Team b = EfFixtureData.MakeTeam(sourceIdBase + 2, $"B{sourceIdBase}");
        Team c = EfFixtureData.MakeTeam(sourceIdBase + 3, $"C{sourceIdBase}");
        Team d = EfFixtureData.MakeTeam(sourceIdBase + 4, $"D{sourceIdBase}");
        Stadium stadium = EfFixtureData.MakeStadium(sourceIdBase + 5, $"St{sourceIdBase}");
        Group groupX = new Group(groupHomeName);
        Group groupY = new Group(groupAwayName);
        Journey journey = Journey.CreateInstance(sourceIdBase);

        MatchGroup sourceHome = EfFixtureData.MakeGroupMatch(sourceIdBase + 10, Date(1), a, b, stadium, groupX, journey);
        sourceHome.SetGoals(2, 0);
        MatchGroup sourceAway = EfFixtureData.MakeGroupMatch(sourceIdBase + 11, Date(1), c, d, stadium, groupY, journey);
        sourceAway.SetGoals(0, 2);

        EfFixtureData.Seed(_factory, a, b, c, d, stadium, groupX, groupY, journey, sourceHome, sourceAway);

        Team eliminationHome = sourceHome.GetWinner() ?? a;
        Team eliminationAway = sourceAway.GetWinner() ?? d;
        _repository.Add(EfFixtureData.MakeEliminationMatch(
            eliminationId, Date(15), eliminationHome, eliminationAway, stadium, phase, sourceHome, sourceAway));
    }
}
