using ProjectCopa;
using ProjectCopa.Exceptions;
using DomainTest.TestDoubles;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public sealed class MatchIncidentTests
{
    private Team _homeTeam = null!;
    private Team _awayTeam = null!;
    private Stadium _stadium = null!;
    private Match _match = null!;

    private static readonly DateTime MatchDate = new DateTime(2022, 11, 21);

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Team(1, "Home Team", 1000, true, Confederation.CONMEBOL);
        _awayTeam = new Team(2, "Away Team", 900, true, Confederation.UEFA);
        _stadium = new Stadium("Test Stadium", "Test City", "A test stadium", 50000);
        _match = new MockMatch(1, MatchDate, _homeTeam, _awayTeam, _stadium);
    }

    [TestMethod]
    public void NewMatchHasNoIncidents()
    {
        Assert.AreEqual(0, _match.Incidents.Count);
    }

    [TestMethod]
    public void AddIncidentAcceptsYellowCardForHomeTeam()
    {
        _match.AddIncident(new YellowCardIncident(1, 30, _homeTeam));

        Assert.AreEqual(1, _match.Incidents.Count);
        Assert.AreEqual(1, _match.CountYellowCards(_homeTeam));
    }

    [TestMethod]
    public void AddIncidentAcceptsRedCardForAwayTeam()
    {
        _match.AddIncident(new RedCardIncident(1, 75, _awayTeam));

        Assert.AreEqual(1, _match.CountRedCards(_awayTeam));
    }

    [TestMethod]
    public void AddIncidentRejectsIncidentForTeamNotInMatch()
    {
        Team stranger = new Team(99, "Stranger", 1500, true, Confederation.AFC);

        Assert.ThrowsException<MatchException>(() =>
            _match.AddIncident(new YellowCardIncident(1, 30, stranger)));
    }

    [TestMethod]
    public void IncidentRejectsNegativeMinute()
    {
        Assert.ThrowsException<MatchException>(() =>
            new YellowCardIncident(1, -1, _homeTeam));
    }

    [TestMethod]
    public void IncidentAcceptsMinuteZero()
    {
        _match.AddIncident(new RedCardIncident(1, 0, _homeTeam));

        Assert.AreEqual(1, _match.CountRedCards(_homeTeam));
    }

    [TestMethod]
    public void CountsAreCorrectByTeamAndType()
    {
        _match.AddIncident(new YellowCardIncident(1, 10, _homeTeam));
        _match.AddIncident(new YellowCardIncident(2, 20, _homeTeam));
        _match.AddIncident(new RedCardIncident(3, 80, _homeTeam));
        _match.AddIncident(new YellowCardIncident(4, 35, _awayTeam));

        Assert.AreEqual(2, _match.CountYellowCards(_homeTeam));
        Assert.AreEqual(1, _match.CountRedCards(_homeTeam));
        Assert.AreEqual(1, _match.CountYellowCards(_awayTeam));
        Assert.AreEqual(0, _match.CountRedCards(_awayTeam));
    }
}
