using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;
using Moq;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public class MatchTests
{

    private Mock<ITeam> _homeTeam = null!;
    private Mock<ITeam> _awayTeam = null!;
    private Mock<IStadium> _stadium = null!;
    private Match _match = null!;

    private static readonly DateTime MatchDate = new DateTime(2022, 11, 21);

    private Match CreateMatchWithData()
    {
        return new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object);
    }
    
    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
        _match = new Match();
    }

    [TestMethod]
    public void MatchCanBeCreatedWithoutData()
    {
        Assert.IsNotNull(_match);
    }

    [TestMethod]
    public void MatchCanGetMatchId()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(1, _match.MatchId);
    }

    [TestMethod]
    public void MatchCanGetMatchDate()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(MatchDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchCanGetHomeTeam()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_homeTeam.Object, _match.HomeTeam);
    }

    [TestMethod]
    public void MatchCanGetAwayTeam()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_awayTeam.Object, _match.AwayTeam);
    }

    [TestMethod]
    public void MatchCanGetHomeGoals()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(0, _match.HomeGoals);
    }

    [TestMethod]
    public void MatchCanGetAwayGoals()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(0, _match.AwayGoals);
    }

    [TestMethod]
    public void MatchCanGetStadium()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_stadium.Object, _match.Stadium);
    }
    
    [TestMethod]
    public void MatchRejectsSameHomeAndAwayTeam()
    {
        var sameTeam = new Mock<ITeam>();
        Assert.ThrowsException<MatchException>(() =>
            new Match(1, MatchDate, sameTeam.Object, sameTeam.Object, _stadium.Object));
    }

    [TestMethod]
    public void MatchRejectsInvalidDate()
    {
        var configurationMock = new Mock<IChampConf>();
        configurationMock.Setup(config => config.StartDate()).Returns(MatchDate);
        var invalidDate = MatchDate.AddDays(-1);

        Assert.ThrowsException<MatchException>(() =>
            new Match(1, invalidDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, configurationMock.Object));
    }

    [TestMethod]
    public void MatchRejectsNullChampionshipConfiguration()
    {
        IChampConf? championshipConfiguration = null;

        Assert.ThrowsException<MatchException>(() => new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, championshipConfiguration!));
    }

    [TestMethod]
    public void MatchCanBeCreatedWithChampionshipConfiguration()
    {
        var championshipConfiguration = new Mock<IChampConf>();
        championshipConfiguration.Setup(config => config.StartDate()).Returns(MatchDate);

        _match = new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, championshipConfiguration.Object);

        championshipConfiguration.Verify(config => config.StartDate(), Times.Once);
        Assert.AreEqual(MatchDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchRejectsNullStadium()
    {
        Assert.ThrowsException<MatchException>(() =>
            new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, null!));
    }

    [TestMethod]
    public void MatchCanChangeStadium()
    {
        _match = CreateMatchWithData();
        var newStadium = new Mock<IStadium>();

        _match.ChangeStadium(newStadium.Object);

        Assert.AreSame(newStadium.Object, _match.Stadium);
    }

    [TestMethod]
    public void MatchReturnsHomeTeamWhenHomeGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 1);
        var winnerTeam = _homeTeam.Object;

        Assert.AreSame(winnerTeam, _match.GetWinner());
    }

    [TestMethod]
    public void MatchRejectsNegativeHomeGoals()
    {
        _match = CreateMatchWithData();

        Assert.ThrowsException<MatchException>(() => _match.SetGoals(homeGoals: -1, awayGoals: 0));
    }

    [TestMethod]
    public void MatchRejectsNegativeAwayGoals()
    {
        _match = CreateMatchWithData();

        Assert.ThrowsException<MatchException>(() => _match.SetGoals(homeGoals: 0, awayGoals: -1));
    }

}
