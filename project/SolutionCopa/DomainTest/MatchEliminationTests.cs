using Moq;
using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public class MatchEliminationTests
{
    private static readonly DateTime MatchDate = new(2026, 6, 21);

    private Mock<ITeam> _homeTeam = null!;
    private Mock<ITeam> _awayTeam = null!;
    private Mock<IStadium> _stadium = null!;
    private Mock<IChampConf> _championshipConfiguration = null!;

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
        _championshipConfiguration = new Mock<IChampConf>();
        _championshipConfiguration.Setup(config => config.StartDate()).Returns(MatchDate);
    }

    [TestMethod]
    public void MatchEliminationCanBeCreatedWithoutData()
    {
        var matchElimination = new MatchElimination();

        Assert.IsNotNull(matchElimination);
    }

    [TestMethod]
    public void MatchEliminationCanGetMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            "A1",
            sourceHomeMatch,
            sourceAwayMatch);

        Assert.AreEqual("A1", matchElimination.MatchCode);
    }

    [TestMethod]
    public void MatchEliminationCanGetMatchHome()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            "A1",
            sourceHomeMatch,
            sourceAwayMatch);

        Assert.AreSame(sourceHomeMatch, matchElimination.MatchHome);
    }

    [TestMethod]
    public void MatchEliminationCanGetMatchAway()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            "A1",
            sourceHomeMatch,
            sourceAwayMatch);

        Assert.AreSame(sourceAwayMatch, matchElimination.MatchAway);
    }

    [TestMethod]
    public void MatchEliminationCanBeCreatedWithChampionshipConfiguration()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            "A1",
            sourceHomeMatch,
            sourceAwayMatch);

        Assert.AreEqual(MatchDate, matchElimination.MatchDate);
    }

    [TestMethod]
    public void MatchEliminationRejectsNullMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                null!,
                sourceHomeMatch,
                sourceAwayMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsWhitespaceMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                "   ",
                sourceHomeMatch,
                sourceAwayMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullHomeSourceMatch()
    {
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                "A1",
                null!,
                sourceAwayMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullAwaySourceMatch()
    {
        var sourceHomeMatch = CreateSourceMatch(100);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                "A1",
                sourceHomeMatch,
                null!));
    }

    [TestMethod]
    public void MatchEliminationRejectsSameSourceMatchForHomeAndAway()
    {
        var sourceMatch = CreateSourceMatch(100);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                "A1",
                sourceMatch,
                sourceMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullMatchCodeWhenUsingChampionshipConfiguration()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                _championshipConfiguration.Object,
                null!,
                sourceHomeMatch,
                sourceAwayMatch));
    }

    private static Match CreateSourceMatch(int matchId)
    {
        var sourceHomeTeam = new Mock<ITeam>();
        var sourceAwayTeam = new Mock<ITeam>();
        var sourceStadium = new Mock<IStadium>();

        return new Match(matchId, MatchDate, sourceHomeTeam.Object, sourceAwayTeam.Object, sourceStadium.Object);
    }
}
