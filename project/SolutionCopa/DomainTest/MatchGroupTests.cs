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

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
        _group = new Mock<IGroup>();
        _journey = new Mock<IJourney>();
        _championshipConfiguration = new Mock<IChampConf>();
        _championshipConfiguration.Setup(config => config.StartDate()).Returns(MatchDate);
    }

    [TestMethod]
    public void MatchGroupCanBeCreatedWithoutData()
    {
        var matchGroup = new MatchGroup();

        Assert.IsNotNull(matchGroup);
    }

    [TestMethod]
    public void MatchGroupCanGetGroup()
    {
        var matchGroup = new MatchGroup(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _group.Object,
            _journey.Object);

        Assert.AreSame(_group.Object, matchGroup.Group);
    }

    [TestMethod]
    public void MatchGroupCanGetJourney()
    {
        var matchGroup = new MatchGroup(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _group.Object,
            _journey.Object);

        Assert.AreSame(_journey.Object, matchGroup.Journey);
    }

    [TestMethod]
    public void MatchGroupCanBeCreatedWithChampionshipConfiguration()
    {
        var matchGroup = new MatchGroup(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            _group.Object,
            _journey.Object);

        Assert.AreEqual(MatchDate, matchGroup.MatchDate);
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroup()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                null!,
                _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourney()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                _group.Object,
                null!));
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroupWithChampionshipConfiguration()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                _championshipConfiguration.Object,
                null!,
                _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourneyWithChampionshipConfiguration()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                _championshipConfiguration.Object,
                _group.Object,
                null!));
    }
}
