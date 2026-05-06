using DomainTest.TestDoubles;
using Moq;
using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace DomainTest;

[TestClass]
public class MatchGroupTests
{
    private static readonly DateTime MatchDate = new(2026, 6, 21);

    private Mock<ITeam> _homeTeam = null!;
    private Mock<ITeam> _awayTeam = null!;
    private Mock<IStadium> _stadium = null!;
    private Mock<IGroup> _group = null!;
    private Mock<IJourney> _journey = null!;
    private Mock<IChampConf> _championshipConfiguration = null!;

    private MatchGroup CreateValidMatchGroup()
    {
        return MatchGroup.CreateInstance(1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _group.Object,
            _journey.Object);
    }

    private MatchGroup CreateValidMatchGroupWithConfiguration()
    {
        return MatchGroup.CreateMatchWithConfig(1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            _group.Object,
            _journey.Object);
    }

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
        _group = new Mock<IGroup>();
        _group.Setup(group => group.Name).Returns("A");
        _journey = new Mock<IJourney>();
        _journey.Setup(journey => journey.Number).Returns(1);
        _championshipConfiguration = new Mock<IChampConf>();
        _championshipConfiguration.Setup(configuration => configuration.StartDate()).Returns(MatchDate);
    }

    [TestMethod]
    public void MatchGroupCanGetGroup()
    {
        var matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_group.Object, matchGroup.Group);
    }

    [TestMethod]
    public void MatchGroupCanGetJourney()
    {
        var matchGroup = CreateValidMatchGroup();
        Assert.AreSame(_journey.Object, matchGroup.Journey);
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
        _championshipConfiguration.Verify(configuration => configuration.StartDate(), Times.Once);
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroup()
    {
        Assert.ThrowsException<MatchException>(() =>
            MatchGroup.CreateInstance(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, null!, _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourney()
    {
        Assert.ThrowsException<MatchException>(() =>
            MatchGroup.CreateInstance(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, _group.Object, null!));
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
