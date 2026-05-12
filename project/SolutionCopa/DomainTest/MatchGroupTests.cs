using ProjectCopa;
using ProjectCopa.Exceptions;

namespace DomainTest;

[TestClass]
public class MatchGroupTests
{
    private static readonly DateTime MatchDate = new DateTime(2026, 6, 21);

    private Team _homeTeam = null!;
    private Team _awayTeam = null!;
    private Stadium _stadium = null!;
    private Group _group = null!;
    private Journey _journey = null!;
    private TournamentSettings _tournamentSettings = null!;

    private MatchGroup CreateValidMatchGroup()
    {
        return MatchGroup.CreateInstance(1,
            MatchDate,
            _homeTeam,
            _awayTeam,
            _stadium,
            _group,
            _journey);
    }

    private MatchGroup CreateValidMatchGroupWithConfiguration()
    {
        return MatchGroup.CreateMatchWithConfig(1,
            MatchDate,
            _homeTeam,
            _awayTeam,
            _stadium,
            _tournamentSettings,
            _group,
            _journey);
    }

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Team(1, name: "Team A", fifaRanking: 1000, isFictitious: true, confederation: Confederation.CONMEBOL);
        _awayTeam = new Team(2, name: "Team B", fifaRanking: 900, isFictitious: true, confederation: Confederation.CONMEBOL);
        _stadium = new Stadium(name: "Test Stadium", city: "Test City", description: "A test stadium", capacity: 50000);
        _group = new Group(name: "A");
        _journey = Journey.CreateInstance(number: 1);
        _tournamentSettings = new TournamentSettings();
    }

    [TestMethod]
    public void MatchGroupCanGetGroup()
    {
        MatchGroup matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_group, matchGroup.Group);
    }

    [TestMethod]
    public void MatchGroupCanGetJourney()
    {
        MatchGroup matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_journey, matchGroup.Journey);
    }

    [TestMethod]
    public void MatchGroupCanBeCreatedWithtournamentSettings()
    {
        MatchGroup matchGroup = CreateValidMatchGroupWithConfiguration();
        Assert.AreEqual(MatchDate, matchGroup.MatchDate);
    }

    [TestMethod]
    public void MatchGroupCallsStartDateWhenCreatedWithConfiguration()
    {
        MatchGroup matchGroup = CreateValidMatchGroupWithConfiguration();
        Assert.AreEqual(MatchDate, matchGroup.MatchDate);
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroup()
    {
        Assert.ThrowsException<MatchException>(() =>
            MatchGroup.CreateInstance(1, MatchDate, _homeTeam, _awayTeam, _stadium, null!, _journey));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourney()
    {
        Assert.ThrowsException<MatchException>(() =>
            MatchGroup.CreateInstance(1, MatchDate, _homeTeam, _awayTeam, _stadium, _group, null!));
    }

    [TestMethod]
    public void MatchGroupSetGoalsSetsHomeGoals()
    {
        MatchGroup matchGroup = CreateValidMatchGroup();
        matchGroup.SetGoals(2, 1);
        Assert.AreEqual(2, matchGroup.HomeGoals);
    }

    [TestMethod]
    public void MatchGroupSetGoalsSetsAwayGoals()
    {
        MatchGroup matchGroup = CreateValidMatchGroup();
        matchGroup.SetGoals(2, 1);
        Assert.AreEqual(1, matchGroup.AwayGoals);
    }
}

