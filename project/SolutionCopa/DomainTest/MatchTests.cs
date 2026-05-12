using ProjectCopa;
using ProjectCopa.Exceptions;
using DomainTest.TestDoubles;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public class MatchTests
{
    private Team _homeTeam = null!;
    private Team _awayTeam = null!;
    private Stadium _stadium = null!;
    private Match _match = null!;

    private static readonly DateTime MatchDate = new DateTime(2022, 11, 21);

    private Match CreateMatchWithData()
    {
        return new MockMatch(1, MatchDate, _homeTeam, _awayTeam, _stadium);
    }

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Team(1, name: "Home Team", fifaRanking: 1000, isFictitious: true, confederation: Confederation.CONMEBOL);
        _awayTeam = new Team(2, name: "Away Team", fifaRanking: 900, isFictitious: true, confederation: Confederation.UEFA);
        _stadium = new Stadium(name: "Test Stadium", city: "Test City", description: "A test stadium", capacity: 50000);
        _match = new MockMatch();
    }

    [TestMethod]
    public void MatchCanBeCreatedWithoutData()
    {
        Assert.IsNotNull(_match);
    }

    [TestMethod]
    public void MatchTypeIsAbstract()
    {
        Assert.IsTrue(typeof(Match).IsAbstract);
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
        Assert.AreSame(_homeTeam, _match.HomeTeam);
    }

    [TestMethod]
    public void MatchCanGetAwayTeam()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_awayTeam, _match.AwayTeam);
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
        Assert.AreSame(_stadium, _match.Stadium);
    }

    [TestMethod]
    public void MatchRejectsSameHomeAndAwayTeam()
    {
        Team sameTeam = new Team(1, name: "Same Team", fifaRanking: 800, isFictitious: true, confederation: Confederation.CONMEBOL);

        Assert.ThrowsException<MatchException>(() =>
            new MockMatch(1, MatchDate, sameTeam, sameTeam, _stadium));
    }

    [TestMethod]
    public void MatchAllowsNullHomeSourceMatchInSourceValidation()
    {
        Match sourceAwayMatch = CreateMatchWithData();

        Assert.IsTrue(MockMatch.CanUseSourceMatches(null, sourceAwayMatch));
    }

    [TestMethod]
    public void MatchAllowsNullAwaySourceMatchInSourceValidation()
    {
        Match sourceHomeMatch = CreateMatchWithData();

        Assert.IsTrue(MockMatch.CanUseSourceMatches(sourceHomeMatch, null));
    }

    [TestMethod]
    public void MatchRejectsInvalidDate()
    {
        TournamentSettings tournamentSettings = new TournamentSettings();
        var invalidDate = MatchDate.AddDays(-1);

        Assert.ThrowsException<MatchException>(() =>
            new MockMatch(1, invalidDate, _homeTeam, _awayTeam, _stadium, tournamentSettings));
    }

    [TestMethod]
    public void MatchRejectsNullChampionshipConfiguration()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MockMatch(1, MatchDate, _homeTeam, _awayTeam, _stadium, null!));
    }

    [TestMethod]
    public void MatchCanBeCreatedWithChampionshipConfiguration()
    {
        DateTime validDate = new DateTime(2026, 6, 21);
        TournamentSettings tournamentSettings = new TournamentSettings();

        _match = new MockMatch(1, validDate, _homeTeam, _awayTeam, _stadium, tournamentSettings);

        Assert.AreEqual(validDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchRejectsNullStadium()
    {
        Assert.ThrowsException<MatchException>(() =>
            new MockMatch(1, MatchDate, _homeTeam, _awayTeam, null!));
    }

    [TestMethod]
    public void MatchCanChangeStadium()
    {
        _match = CreateMatchWithData();
        Stadium newStadium = new Stadium(name: "New Stadium", city: "New City", description: "A new stadium", capacity: 60000);

        _match.ChangeStadium(newStadium);

        Assert.AreSame(newStadium, _match.Stadium);
    }

    [TestMethod]
    public void MatchRejectsNullStadiumWhenChangingStadium()
    {
        _match = CreateMatchWithData();

        Assert.ThrowsException<MatchException>(() => _match.ChangeStadium(null!));
    }

    [TestMethod]
    public void MatchReturnsHomeTeamWhenHomeGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 1);
        Team winnerTeam = _homeTeam;

        Assert.AreSame(winnerTeam, _match.GetWinner());
    }

    [TestMethod]
    public void MatchReturnsAwayTeamWhenAwayGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 2);
        Team winnerTeam = _awayTeam;

        Assert.AreSame(winnerTeam, _match.GetWinner());
    }

    [TestMethod]
    public void MatchReturnsNoWinnerWhenGoalsAreEqual()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 2);

        Assert.IsNull(_match.GetWinner());
    }

    [TestMethod]
    public void MatchRejectsDateEarlierThanDefaultTournamentStartDate()
    {
        var invalidDate = MatchDate.AddDays(-1);

        Assert.ThrowsException<MatchException>(() =>
            new MockMatch(1, invalidDate, _homeTeam, _awayTeam, _stadium));
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

    [TestMethod]
    public void SetHomeGoals_SetsHomeGoalsToGivenValue()
    {
        _match = CreateMatchWithData();
        _match.SetHomeGoals(3);
        Assert.AreEqual(3, _match.HomeGoals);
    }

    [TestMethod]
    public void SetHomeGoals_RejectsNegativeValue()
    {
        _match = CreateMatchWithData();
        Assert.ThrowsException<MatchException>(() => _match.SetHomeGoals(-1));
    }

    [TestMethod]
    public void SetAwayGoals_SetsAwayGoalsToGivenValue()
    {
        _match = CreateMatchWithData();
        _match.SetAwayGoals(2);
        Assert.AreEqual(2, _match.AwayGoals);
    }

    [TestMethod]
    public void SetAwayGoals_RejectsNegativeValue()
    {
        _match = CreateMatchWithData();
        Assert.ThrowsException<MatchException>(() => _match.SetAwayGoals(-1));
    }

    [TestMethod]
    public void AddHomeGoal_IncrementsHomeGoalsByOne()
    {
        _match = CreateMatchWithData();
        _match.AddHomeGoal();
        Assert.AreEqual(1, _match.HomeGoals);
    }

    [TestMethod]
    public void AddHomeGoal_CanBeCalledMultipleTimes()
    {
        _match = CreateMatchWithData();
        _match.AddHomeGoal();
        _match.AddHomeGoal();
        Assert.AreEqual(2, _match.HomeGoals);
    }

    [TestMethod]
    public void AddAwayGoal_IncrementsAwayGoalsByOne()
    {
        _match = CreateMatchWithData();
        _match.AddAwayGoal();
        Assert.AreEqual(1, _match.AwayGoals);
    }

    [TestMethod]
    public void AddAwayGoal_CanBeCalledMultipleTimes()
    {
        _match = CreateMatchWithData();
        _match.AddAwayGoal();
        _match.AddAwayGoal();
        Assert.AreEqual(2, _match.AwayGoals);
    }

    [TestMethod]
    public void RemoveHomeGoal_DecrementsHomeGoalsByOne()
    {
        _match = CreateMatchWithData();
        _match.AddHomeGoal();
        _match.RemoveHomeGoal();
        Assert.AreEqual(0, _match.HomeGoals);
    }

    [TestMethod]
    public void RemoveHomeGoal_ThrowsWhenHomeGoalsIsZero()
    {
        _match = CreateMatchWithData();
        Assert.ThrowsException<MatchException>(() => _match.RemoveHomeGoal());
    }

    [TestMethod]
    public void RemoveAwayGoal_DecrementsAwayGoalsByOne()
    {
        _match = CreateMatchWithData();
        _match.AddAwayGoal();
        _match.RemoveAwayGoal();
        Assert.AreEqual(0, _match.AwayGoals);
    }

    [TestMethod]
    public void RemoveAwayGoal_ThrowsWhenAwayGoalsIsZero()
    {
        _match = CreateMatchWithData();
        Assert.ThrowsException<MatchException>(() => _match.RemoveAwayGoal());
    }

    [TestMethod]
    public void HasHomeTeamWon_ReturnsTrueWhenHomeGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 1);
        Assert.IsTrue(_match.HasHomeTeamWon());
    }

    [TestMethod]
    public void HasHomeTeamWon_ReturnsFalseWhenAwayGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 2);
        Assert.IsFalse(_match.HasHomeTeamWon());
    }

    [TestMethod]
    public void HasHomeTeamWon_ReturnsFalseWhenGoalsAreEqual()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 1);
        Assert.IsFalse(_match.HasHomeTeamWon());
    }

    [TestMethod]
    public void HasAwayTeamWon_ReturnsTrueWhenAwayGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 0, awayGoals: 3);
        Assert.IsTrue(_match.HasAwayTeamWon());
    }

    [TestMethod]
    public void HasAwayTeamWon_ReturnsFalseWhenHomeGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 3, awayGoals: 0);
        Assert.IsFalse(_match.HasAwayTeamWon());
    }

    [TestMethod]
    public void HasAwayTeamWon_ReturnsFalseWhenGoalsAreEqual()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 2);
        Assert.IsFalse(_match.HasAwayTeamWon());
    }

    [TestMethod]
    public void IsDraw_ReturnsTrueWhenGoalsAreEqual()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 1);
        Assert.IsTrue(_match.IsDraw());
    }

    [TestMethod]
    public void IsDraw_ReturnsFalseWhenHomeGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 0);
        Assert.IsFalse(_match.IsDraw());
    }

    [TestMethod]
    public void IsDraw_ReturnsFalseWhenAwayGoalsAreGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 0, awayGoals: 1);
        Assert.IsFalse(_match.IsDraw());
    }

    [TestMethod]
    public void GetGoalDifferenceForHome_ReturnsHomeMinusAway()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 3, awayGoals: 1);
        Assert.AreEqual(2, _match.GetGoalDifferenceForHome());
    }

    [TestMethod]
    public void GetGoalDifferenceForHome_ReturnsNegativeWhenAwayIsGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 4);
        Assert.AreEqual(-3, _match.GetGoalDifferenceForHome());
    }

    [TestMethod]
    public void GetGoalDifferenceForHome_ReturnsZeroWhenDraw()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 2);
        Assert.AreEqual(0, _match.GetGoalDifferenceForHome());
    }

    [TestMethod]
    public void GetGoalDifferenceForAway_ReturnsAwayMinusHome()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 1, awayGoals: 3);
        Assert.AreEqual(2, _match.GetGoalDifferenceForAway());
    }

    [TestMethod]
    public void GetGoalDifferenceForAway_ReturnsNegativeWhenHomeIsGreater()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 4, awayGoals: 1);
        Assert.AreEqual(-3, _match.GetGoalDifferenceForAway());
    }

    [TestMethod]
    public void GetGoalDifferenceForAway_ReturnsZeroWhenDraw()
    {
        _match = CreateMatchWithData();
        _match.SetGoals(homeGoals: 2, awayGoals: 2);
        Assert.AreEqual(0, _match.GetGoalDifferenceForAway());
    }

    [TestMethod]
    public void MatchCanChangeDateToValidTournamentDate()
    {
        _match = CreateMatchWithData();
        DateTime newDate = new DateTime(2022, 11, 25);

        _match.ChangeDate(newDate);

        Assert.AreEqual(newDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchRejectsChangeDateBeforeTournamentStart()
    {
        _match = CreateMatchWithData();
        DateTime invalidDate = new DateTime(2022, 11, 19);

        Assert.ThrowsException<MatchException>(() => _match.ChangeDate(invalidDate));
    }
}
