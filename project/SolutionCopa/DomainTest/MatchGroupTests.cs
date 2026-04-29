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

    private MatchGroup CreateValidMatchGroup(IGroup group, IJourney journey)
    {
        return new MatchGroup(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            group,
            journey);
    }

    private MatchGroup CreateValidMatchGroupWithConfiguration(IGroup group, IJourney journey)
    {
        return new MatchGroup(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            group,
            journey);
    }
    
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
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);

        Assert.AreSame(_group.Object, matchGroup.Group);
    }

    [TestMethod]
    public void MatchGroupCanGetJourney()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);

        Assert.AreSame(_journey.Object, matchGroup.Journey);
    }

    [TestMethod]
    public void MatchGroupCanGetHomeTeam()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);

        Assert.AreSame(_homeTeam.Object, matchGroup.HomeTeam);
    }

    [TestMethod]
    public void MatchGroupCanGetAwayTeam()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);

        Assert.AreSame(_awayTeam.Object, matchGroup.AwayTeam);
    }

    [TestMethod]
    public void MatchGroupCanGetStadium()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);

        Assert.AreSame(_stadium.Object, matchGroup.Stadium);
    }

    [TestMethod]
    public void MatchGroupCanBeCreatedWithChampionshipConfiguration()
    {
        var matchGroup = CreateValidMatchGroupWithConfiguration(_group.Object, _journey.Object);

        Assert.AreEqual(MatchDate, matchGroup.MatchDate);
    }

    [TestMethod]
    public void MatchGroupRejectsSameHomeAndAwayTeam()
    {
        var sameTeam = new Mock<ITeam>();

        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                sameTeam.Object,
                sameTeam.Object,
                _stadium.Object,
                _group.Object,
                _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullStadium()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MatchGroup(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                null!,
                _group.Object,
                _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroup()
    {
        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchGroup(null!, _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourney()
    {
        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchGroup(_group.Object, null!));
    }

    [TestMethod]
    public void MatchGroupRejectsNullGroupWithChampionshipConfiguration()
    {
        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchGroupWithConfiguration(null!, _journey.Object));
    }

    [TestMethod]
    public void MatchGroupRejectsNullJourneyWithChampionshipConfiguration()
    {
        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchGroupWithConfiguration(_group.Object, null!));
    }

    // Inheritance: scoring behaviors
    [TestMethod]
    public void MatchGroup_SetHomeGoals_SetsValue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetHomeGoals(2);
        Assert.AreEqual(2, matchGroup.HomeGoals);
    }

    [TestMethod]
    public void MatchGroup_SetAwayGoals_SetsValue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetAwayGoals(1);
        Assert.AreEqual(1, matchGroup.AwayGoals);
    }

    [TestMethod]
    public void MatchGroup_AddHomeGoal_Increments()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.AddHomeGoal();
        Assert.AreEqual(1, matchGroup.HomeGoals);
    }

    [TestMethod]
    public void MatchGroup_AddAwayGoal_Increments()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.AddAwayGoal();
        Assert.AreEqual(1, matchGroup.AwayGoals);
    }

    [TestMethod]
    public void MatchGroup_RemoveHomeGoal_Decrements()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.AddHomeGoal();
        matchGroup.RemoveHomeGoal();
        Assert.AreEqual(0, matchGroup.HomeGoals);
    }

    [TestMethod]
    public void MatchGroup_RemoveAwayGoal_Decrements()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.AddAwayGoal();
        matchGroup.RemoveAwayGoal();
        Assert.AreEqual(0, matchGroup.AwayGoals);
    }

    [TestMethod]
    public void MatchGroup_HasHomeTeamWon_ReturnsTrue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetGoals(2, 0);
        Assert.IsTrue(matchGroup.HasHomeTeamWon());
    }

    [TestMethod]
    public void MatchGroup_HasAwayTeamWon_ReturnsTrue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetGoals(0, 2);
        Assert.IsTrue(matchGroup.HasAwayTeamWon());
    }

    [TestMethod]
    public void MatchGroup_IsDraw_ReturnsTrueOnEqualGoals()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetGoals(1, 1);
        Assert.IsTrue(matchGroup.IsDraw());
    }

    [TestMethod]
    public void MatchGroup_GetGoalDifferenceForHome_ReturnsCorrectValue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetGoals(3, 1);
        Assert.AreEqual(2, matchGroup.GetGoalDifferenceForHome());
    }

    [TestMethod]
    public void MatchGroup_GetGoalDifferenceForAway_ReturnsCorrectValue()
    {
        var matchGroup = CreateValidMatchGroup(_group.Object, _journey.Object);
        matchGroup.SetGoals(1, 3);
        Assert.AreEqual(2, matchGroup.GetGoalDifferenceForAway());
    }
}
