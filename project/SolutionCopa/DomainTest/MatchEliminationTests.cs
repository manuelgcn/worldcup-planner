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
    public void MatchEliminationCanGetPhase()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, phase: Phase.SemiFinal);

        Assert.AreEqual(Phase.SemiFinal, matchElimination.Phase);
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
    public void MatchEliminationCanGetPhaseWhenUsingChampionshipConfiguration()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchEliminationWithConfiguration(sourceHomeMatch, sourceAwayMatch, phase: Phase.Final);

        Assert.AreEqual(Phase.Final, matchElimination.Phase);
    }

    [DataTestMethod]
    [DataRow(Phase.Final)]
    [DataRow(Phase.SemiFinal)]
    [DataRow(Phase.QuarterFinals)]
    [DataRow(Phase.RoundOf16)]
    [DataRow(Phase.RoundOf32)]
    public void MatchEliminationAcceptsEveryDefinedPhase(Phase phase)
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        var matchElimination = CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, phase: phase);

        Assert.AreEqual(phase, matchElimination.Phase);
    }

    [TestMethod]
    public void MatchEliminationRejectsInvalidPhase()
    {
        var sourceHomeMatch = CreateSourceMatch(100);
        var sourceAwayMatch = CreateSourceMatch(200);

        Assert.ThrowsException<MatchException>(() =>
            CreateValidMatchElimination(sourceHomeMatch, sourceAwayMatch, phase: (Phase)999));
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

    private MatchElimination CreateValidMatchElimination(
        Match matchHome,
        Match matchAway,
        string matchCode = "A1",
        Phase phase = Phase.RoundOf16)
    {
        return new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            phase,
            matchCode,
            matchHome,
            matchAway);
    }

    private MatchElimination CreateValidMatchEliminationWithConfiguration(
        Match matchHome,
        Match matchAway,
        string matchCode = "A1",
        Phase phase = Phase.RoundOf16)
    {
        return new MatchElimination(
            1,
            MatchDate,
            _homeTeam.Object,
            _awayTeam.Object,
            _stadium.Object,
            _championshipConfiguration.Object,
            phase,
            matchCode,
            matchHome,
            matchAway);
    }

    // Inheritance: scoring behaviors
    [TestMethod]
    public void MatchElimination_SetHomeGoals_SetsValue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetHomeGoals(2);
        Assert.AreEqual(2, matchElimination.HomeGoals);
    }

    [TestMethod]
    public void MatchElimination_SetAwayGoals_SetsValue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetAwayGoals(1);
        Assert.AreEqual(1, matchElimination.AwayGoals);
    }

    [TestMethod]
    public void MatchElimination_AddHomeGoal_Increments()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.AddHomeGoal();
        Assert.AreEqual(1, matchElimination.HomeGoals);
    }

    [TestMethod]
    public void MatchElimination_AddAwayGoal_Increments()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.AddAwayGoal();
        Assert.AreEqual(1, matchElimination.AwayGoals);
    }

    [TestMethod]
    public void MatchElimination_RemoveHomeGoal_Decrements()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.AddHomeGoal();
        matchElimination.RemoveHomeGoal();
        Assert.AreEqual(0, matchElimination.HomeGoals);
    }

    [TestMethod]
    public void MatchElimination_RemoveAwayGoal_Decrements()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.AddAwayGoal();
        matchElimination.RemoveAwayGoal();
        Assert.AreEqual(0, matchElimination.AwayGoals);
    }

    [TestMethod]
    public void MatchElimination_HasHomeTeamWon_ReturnsTrue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetGoals(2, 0);
        Assert.IsTrue(matchElimination.HasHomeTeamWon());
    }

    [TestMethod]
    public void MatchElimination_HasAwayTeamWon_ReturnsTrue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetGoals(0, 2);
        Assert.IsTrue(matchElimination.HasAwayTeamWon());
    }

    [TestMethod]
    public void MatchElimination_IsDraw_ReturnsTrueOnEqualGoals()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetGoals(1, 1);
        Assert.IsTrue(matchElimination.IsDraw());
    }

    [TestMethod]
    public void MatchElimination_GetGoalDifferenceForHome_ReturnsCorrectValue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetGoals(3, 1);
        Assert.AreEqual(2, matchElimination.GetGoalDifferenceForHome());
    }

    [TestMethod]
    public void MatchElimination_GetGoalDifferenceForAway_ReturnsCorrectValue()
    {
        var sourceHome = CreateSourceMatch(100);
        var sourceAway = CreateSourceMatch(200);
        var matchElimination = CreateValidMatchElimination(sourceHome, sourceAway);
        matchElimination.SetGoals(1, 3);
        Assert.AreEqual(2, matchElimination.GetGoalDifferenceForAway());
    }
}
