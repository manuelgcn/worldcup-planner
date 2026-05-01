using DomainTest.TestDoubles;
using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace DomainTest;

[TestClass]
public class MatchGroupTests
{
    private static readonly DateTime MatchDate = new(2026, 6, 21);

    private ITeam _homeTeam = null!;
    private ITeam _awayTeam = null!;
    private IStadium _stadium = null!;
    private IGroup _group = null!;
    private IJourney _journey = null!;
    private DummyChampConf _championshipConfiguration = null!;

    private MatchGroup CreateValidMatchGroup()
    {
        return new MatchGroup(
            1,
            MatchDate,
            _homeTeam,
            _awayTeam,
            _stadium,
            _group,
            _journey);
    }

    private MatchGroup CreateValidMatchGroupWithConfiguration()
    {
        return new MatchGroup(
            1,
            MatchDate,
            _homeTeam,
            _awayTeam,
            _stadium,
            _championshipConfiguration,
            _group,
            _journey);
    }

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new StubTeam(1, "Home Team");
        _awayTeam = new StubTeam(2, "Away Team");
        _stadium = new StubStadium(1, "Main Stadium");
        _group = new StubGroup("A");
        _journey = new StubJourney(1);
        _championshipConfiguration = new DummyChampConf(MatchDate);
    }

    [TestMethod]
    public void MatchGroupCanGetGroup()
    {
        var matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_group, matchGroup.Group);
    }

    [TestMethod]
    public void MatchGroupCanGetJourney()
    {
        var matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_journey, matchGroup.Journey);
    }

    [TestMethod]
    public void MatchGroupCanBeCreatedWithChampionshipConfiguration()
    {
        var matchGroup = CreateValidMatchGroupWithConfiguration();
        Assert.AreEqual(MatchDate, matchGroup.MatchDate);
    }

    [TestMethod]
    public void MatchGroupCallsStartDateWhenCreatedWithConfiguration()
    {
        CreateValidMatchGroupWithConfiguration();
        Assert.AreEqual(1, _championshipConfiguration.StartDateCalls);
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroup()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(1, MatchDate, _homeTeam, _awayTeam, _stadium, null!, _journey));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourney()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(1, MatchDate, _homeTeam, _awayTeam, _stadium, _group, null!));
    }

    [TestMethod]
    public void MatchGroupSetGoalsSetsHomeGoals()
    {
        var matchGroup = CreateValidMatchGroup();
        matchGroup.SetGoals(2, 1);
        Assert.AreEqual(2, matchGroup.HomeGoals);
    }

    [TestMethod]
    public void MatchGroupSetGoalsSetsAwayGoals()
    {
        var matchGroup = CreateValidMatchGroup();
        matchGroup.SetGoals(2, 1);
        Assert.AreEqual(1, matchGroup.AwayGoals);
    }
}
