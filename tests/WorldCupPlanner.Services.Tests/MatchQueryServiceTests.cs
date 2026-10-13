using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class MatchQueryServiceTests
{
    private MockMatchRepository _repository = null!;
    private IMatchQueryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new MockMatchRepository();
        _service = new MatchQueryService(_repository);
    }

    [TestMethod]
    public void Search_WithoutFilters_ReturnsEachMatchExactlyOnce()
    {
        _repository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "Centenario")));
        _repository.Add(EliminationMatch(2, Date(20), Phase.RoundOf16, Stadium(2, "Maracana")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto());

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2, result.Select(item => item.MatchId).Distinct().Count());
    }

    [TestMethod]
    public void Search_FromDate_ReturnsOnlyMatchesOnOrAfterThatDay()
    {
        _repository.Add(GroupMatch(1, Date(5), "A", Stadium(1, "St1")));
        _repository.Add(GroupMatch(2, Date(10), "A", Stadium(1, "St1")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto { From = Date(8) });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_ToDate_IncludesTheWholeRequestedDay()
    {
        _repository.Add(GroupMatch(1, Date(10).AddHours(21), "A", Stadium(1, "St1")));
        _repository.Add(GroupMatch(2, Date(11).AddHours(9), "A", Stadium(1, "St1")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto { To = Date(10) });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_ByStadium_ReturnsOnlyMatchesAtThatStadium()
    {
        _repository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "Centenario")));
        _repository.Add(GroupMatch(2, Date(10), "A", Stadium(2, "Maracana")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto { StadiumId = 2 });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_ByGroup_ReturnsOnlyMatchesOfThatGroup()
    {
        _repository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "St1")));
        _repository.Add(GroupMatch(2, Date(10), "B", Stadium(1, "St1")));
        _repository.Add(EliminationMatch(3, Date(20), Phase.RoundOf16, Stadium(1, "St1")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto { GroupName = "A" });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_ByPhase_ReturnsOnlyEliminationMatchesOfThatPhase()
    {
        _repository.Add(EliminationMatch(1, Date(20), Phase.RoundOf16, Stadium(1, "St1")));
        _repository.Add(EliminationMatch(2, Date(21), Phase.QuarterFinals, Stadium(1, "St1")));
        _repository.Add(GroupMatch(3, Date(10), "A", Stadium(1, "St1")));

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto { Phase = Phase.RoundOf16 });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_CombinedFilters_AppliesEveryConditionWithAnd()
    {
        _repository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "St1")));
        _repository.Add(GroupMatch(2, Date(10), "B", Stadium(1, "St1")));
        _repository.Add(GroupMatch(3, Date(20), "A", Stadium(1, "St1")));

        IReadOnlyCollection<MatchListItemDto> result =
            _service.Search(new MatchFilterDto { GroupName = "A", From = Date(15) });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(3, result.Single().MatchId);
    }

    [TestMethod]
    public void Search_MapsFlatFieldsForGroupAndEliminationMatches()
    {
        MatchGroup group = GroupMatch(1, Date(10), "A", Stadium(1, "Centenario"));
        group.SetGoals(2, 1);
        _repository.Add(group);
        MatchElimination elimination = EliminationMatch(2, Date(20), Phase.RoundOf16, Stadium(2, "Maracana"));
        elimination.MarkAsSimulated();
        _repository.Add(elimination);

        IReadOnlyCollection<MatchListItemDto> result = _service.Search(new MatchFilterDto());

        MatchListItemDto groupItem = result.Single(item => item.MatchId == 1);
        Assert.AreEqual("H1", groupItem.HomeTeamName);
        Assert.AreEqual("A1", groupItem.AwayTeamName);
        Assert.AreEqual("Centenario", groupItem.StadiumName);
        Assert.AreEqual("A", groupItem.GroupName);
        Assert.IsNull(groupItem.Phase);
        Assert.AreEqual(2, groupItem.HomeGoals);
        Assert.AreEqual(1, groupItem.AwayGoals);

        MatchListItemDto eliminationItem = result.Single(item => item.MatchId == 2);
        Assert.IsNull(eliminationItem.GroupName);
        Assert.AreEqual(Phase.RoundOf16, eliminationItem.Phase);
        Assert.IsTrue(eliminationItem.WasSimulated);
        Assert.AreEqual("Maracana", eliminationItem.StadiumName);
    }

    [TestMethod]
    [ExpectedException(typeof(MatchServicesException))]
    public void Search_WhenFromIsAfterTo_ThrowsServiceException()
    {
        _service.Search(new MatchFilterDto { From = Date(20), To = Date(10) });
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

    private static Team Team(int id, string name) =>
        new Team(id, name, 1000 + id, false, Confederation.CONMEBOL);

    private static Stadium Stadium(int id, string name) =>
        new Stadium(name, "Test City", null, 50000) { Id = id };

    private static MatchGroup GroupMatch(int id, DateTime date, string groupName, Stadium stadium)
    {
        MatchGroup match = MatchGroup.CreateInstance(
            id, date, Team(id * 10 + 1, "H" + id), Team(id * 10 + 2, "A" + id), stadium,
            new Group(groupName), Journey.CreateInstance(id));
        match.SetMatchCode("M" + id);
        return match;
    }

    private static MatchElimination EliminationMatch(int id, DateTime date, Phase phase, Stadium stadium)
    {
        // CreateInitialRound no exige partidos fuente: ideal para construir eliminatorias livianas en tests.
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026, 1, 1), 3, 3, 0, 0, 0, 0);
        return MatchElimination.CreateInitialRound(
            id, date, Team(id * 10 + 1, "H" + id), Team(id * 10 + 2, "A" + id), stadium, settings, phase, "E" + id);
    }
}
