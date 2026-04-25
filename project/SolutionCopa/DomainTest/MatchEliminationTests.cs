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
    public void MatchEliminationCanGetMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch);

        Assert.AreEqual("A1", matchElimination.MatchCode);
    }

    [TestMethod]
    public void MatchEliminationCanGetMatchHome()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch);

        Assert.AreSame(sourceHomeMatch, matchElimination.MatchHome);
    }

    [TestMethod]
    public void MatchEliminationCanGetMatchAway()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch);

        Assert.AreSame(sourceAwayMatch, matchElimination.MatchAway);
    }

    [TestMethod]
    public void MatchEliminationCanBeCreatedWithChampionshipConfiguration()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchEliminationWithConfiguration(sourceHomeMatch, sourceAwayMatch);

        Assert.AreEqual(MatchDate, matchElimination.MatchDate);
    }

    [TestMethod]
    public void MatchEliminationRejectsNullMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, matchCode: null!));
    }

    [TestMethod]
    public void MatchEliminationRejectsWhitespaceMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, matchCode: "   "));
    }

    [TestMethod]
    public void MatchEliminationRejectsEmptyMatchCode()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, matchCode: string.Empty));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullHomeSourceMatch()
    {
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(null!, sourceAwayMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullAwaySourceMatch()
    {
        var sourceHomeMatch = CreateSourceMatch(100);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceHomeMatch, null!));
    }

    [TestMethod]
    public void MatchEliminationRejectsSameSourceMatchForHomeAndAway()
    {
        var sourceMatch = CreateSourceMatch(100);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceMatch, sourceMatch));
    }

    [TestMethod]
    public void MatchEliminationRejectsNullMatchCodeWhenUsingChampionshipConfiguration()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchEliminationWithConfiguration(sourceHomeMatch, sourceAwayMatch, matchCode: null!));
    }

    private static Match CreateSourceMatch(int matchId)
    {
        var sourceHomeTeam = new Mock<ITeam>();
        var sourceAwayTeam = new Mock<ITeam>();
        var sourceStadium = new Mock<IStadium>();

        return new Match(matchId, MatchDate, sourceHomeTeam.Object, sourceAwayTeam.Object, sourceStadium.Object);
    }

    private MatchElimination CreateValidMatchElimination(Match matchHome, Match matchAway, string matchCode = "A1")
    {
        return new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            matchCode,
            matchHome,
            matchAway);
    }

    private MatchElimination CreateValidMatchEliminationWithConfiguration(
        Match matchHome,
        Match matchAway,
        string matchCode = "A1")
    {
        return new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            matchCode,
            matchHome,
            matchAway);
    }
}
