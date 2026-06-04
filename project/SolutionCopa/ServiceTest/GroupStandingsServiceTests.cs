using DataAccess.Repositories;
using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class GroupStandingsServiceTests
{
    private const int DefaultSettingsId = 1;
    private const int DefaultSeed = 0;
    private const string GroupName = "A";

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private MockMatchRepository _matchRepository = null!;
    private MockGroupRepository _groupRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private GroupStandingsService _service = null!;

    private Team _teamAlpha = null!;
    private Team _teamBravo = null!;
    private Team _teamCharlie = null!;
    private Team _teamDelta = null!;
    private Stadium _stadium = null!;
    private Group _group = null!;

    [TestInitialize]
    public void Setup()
    {
        _matchRepository = new MockMatchRepository();
        _groupRepository = new MockGroupRepository();
        _settingsRepository = new MockTournamentSettingsRepository();
        _settingsRepository.Add(CreateDefaultSettings());

        _teamAlpha = new Team(1, "Alpha", 1500, false, Confederation.UEFA);
        _teamBravo = new Team(2, "Bravo", 1500, false, Confederation.UEFA);
        _teamCharlie = new Team(3, "Charlie", 1500, false, Confederation.UEFA);
        _teamDelta = new Team(4, "Delta", 1500, false, Confederation.UEFA);
        _stadium = new Stadium("Centenario", "Montevideo", null, 60000) { Id = 1 };

        _group = new Group(GroupName);
        _group.AddTeam(_teamAlpha);
        _group.AddTeam(_teamBravo);
        _group.AddTeam(_teamCharlie);
        _group.AddTeam(_teamDelta);
        _groupRepository.Add(_group);

        _service = new GroupStandingsService(_matchRepository, _groupRepository, _settingsRepository);
    }

    [TestMethod]
    public void GetStandings_WithNoMatches_ReturnsFourTeamsWithZeroStats()
    {
        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.AreEqual(4, standings.Count);
        Assert.IsTrue(standings.All(standing => standing.GamesPlayed == 0));
        Assert.IsTrue(standings.All(standing => standing.Points == 0));
        Assert.IsTrue(standings.All(standing => standing.GoalsFor == 0));
    }

    [TestMethod]
    public void GetStandings_OrdersByPointsDescending()
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 2, awayGoals: 0);
        AddPlayedMatch(matchId: 2, home: _teamCharlie, away: _teamDelta, homeGoals: 1, awayGoals: 0);
        AddPlayedMatch(matchId: 3, home: _teamAlpha, away: _teamCharlie, homeGoals: 1, awayGoals: 0);

        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.AreEqual(_teamAlpha.Id, standings[0].Team.Id);
        Assert.AreEqual(6, standings[0].Points);
    }

    [TestMethod]
    public void GetStandings_TieBreaksByGoalDifferenceWhenPointsEqual()
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 5, awayGoals: 0);
        AddPlayedMatch(matchId: 2, home: _teamCharlie, away: _teamDelta, homeGoals: 1, awayGoals: 0);

        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.AreEqual(_teamAlpha.Id, standings[0].Team.Id);
        Assert.AreEqual(5, standings[0].GoalDifference);
        Assert.AreEqual(_teamCharlie.Id, standings[1].Team.Id);
        Assert.AreEqual(1, standings[1].GoalDifference);
    }

    [TestMethod]
    public void GetStandings_TieBreaksByGoalsForWhenPointsAndGoalDifferenceEqual()
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 3, awayGoals: 1);
        AddPlayedMatch(matchId: 2, home: _teamCharlie, away: _teamDelta, homeGoals: 4, awayGoals: 2);

        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.AreEqual(_teamCharlie.Id, standings[0].Team.Id);
        Assert.AreEqual(_teamAlpha.Id, standings[1].Team.Id);
    }

    [TestMethod]
    public void GetStandings_CountsWinsDrawsLossesCorrectly()
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 2, awayGoals: 1);
        AddPlayedMatch(matchId: 2, home: _teamAlpha, away: _teamCharlie, homeGoals: 1, awayGoals: 1);
        AddPlayedMatch(matchId: 3, home: _teamAlpha, away: _teamDelta, homeGoals: 0, awayGoals: 1);

        GroupStandingDto alpha = _service.GetStandings(GroupName).First(standing => standing.Team.Id == _teamAlpha.Id);

        Assert.AreEqual(3, alpha.GamesPlayed);
        Assert.AreEqual(1, alpha.Wins);
        Assert.AreEqual(1, alpha.Draws);
        Assert.AreEqual(1, alpha.Losses);
        Assert.AreEqual(4, alpha.Points);
    }

    [TestMethod]
    public void GetStandings_AccumulatesGoalsForAndAgainst()
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 3, awayGoals: 2);
        AddPlayedMatch(matchId: 2, home: _teamCharlie, away: _teamAlpha, homeGoals: 1, awayGoals: 4);

        GroupStandingDto alpha = _service.GetStandings(GroupName).First(standing => standing.Team.Id == _teamAlpha.Id);

        Assert.AreEqual(7, alpha.GoalsFor);
        Assert.AreEqual(3, alpha.GoalsAgainst);
        Assert.AreEqual(4, alpha.GoalDifference);
    }

    [TestMethod]
    public void GetStandings_WithUnknownGroupName_Throws()
    {
        Assert.ThrowsException<GroupServicesException>(() => _service.GetStandings("ZZ"));
    }

    [TestMethod]
    public void GetStandings_IgnoresMatchesFromOtherGroups()
    {
        Group otherGroup = new Group("B");
        Team otherHome = new Team(99, "Other-H", 1500, false, Confederation.UEFA);
        Team otherAway = new Team(98, "Other-A", 1500, false, Confederation.UEFA);
        otherGroup.AddTeam(otherHome);
        otherGroup.AddTeam(otherAway);
        _groupRepository.Add(otherGroup);

        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 2, awayGoals: 0);
        Journey journey = Journey.CreateInstance(1);
        MatchGroup otherMatch = MatchGroup.CreateInstance(id:2, matchDate: DefaultStartDate, homeTeam: otherHome, awayTeam: otherAway, stadium: _stadium, group: otherGroup, journey: journey);
        otherMatch.SetGoals(5, 0);
        _matchRepository.Add(otherMatch);

        GroupStandingDto alpha = _service.GetStandings(GroupName).First(standing => standing.Team.Id == _teamAlpha.Id);
        Assert.AreEqual(3, alpha.Points);
        Assert.AreEqual(2, alpha.GoalsFor);
    }

    [TestMethod]
    public void IsGroupStageComplete_WithoutAnyMatches_ReturnsFalse()
    {
        Assert.IsFalse(_service.IsGroupStageComplete());
    }

    [TestMethod]
    public void IsGroupStageComplete_WhenAllMatchesHaveAtLeastOneGoal_ReturnsTrue()
    {
        SeedFullGroupStage(allMatchesHaveGoals: true);

        Assert.IsTrue(_service.IsGroupStageComplete());
    }

    [TestMethod]
    public void IsGroupStageComplete_WhenAnyMatchHasZeroGoals_ReturnsFalse()
    {
        SeedFullGroupStage(allMatchesHaveGoals: false);

        Assert.IsFalse(_service.IsGroupStageComplete());
    }

    [TestMethod]
    public void PendingGroupStageMatchCount_WhenAllPlayed_ReturnsZero()
    {
        SeedFullGroupStage(allMatchesHaveGoals: true);

        Assert.AreEqual(0, _service.PendingGroupStageMatchCount());
    }

    [TestMethod]
    public void PendingGroupStageMatchCount_CountsMatchesWithZeroTotalGoals()
    {
        SeedFullGroupStage(allMatchesHaveGoals: false);

        Assert.AreEqual(1, _service.PendingGroupStageMatchCount());
    }

    private void AddPlayedMatch(int matchId, Team home, Team away, int homeGoals, int awayGoals)
    {
        Journey journey = Journey.CreateInstance(1);
        MatchGroup match = MatchGroup.CreateInstance(
            id: matchId,
            matchDate: DefaultStartDate.AddDays(matchId),
            homeTeam: home,
            awayTeam: away,
            stadium: _stadium,
            group: _group,
            journey: journey);
        match.SetGoals(homeGoals, awayGoals);
        _matchRepository.Add(match);
    }

    private void SeedFullGroupStage(bool allMatchesHaveGoals)
    {
        AddPlayedMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 1, awayGoals: 0);
        AddPlayedMatch(matchId: 2, home: _teamCharlie, away: _teamDelta, homeGoals: 2, awayGoals: 1);
        AddPlayedMatch(matchId: 3, home: _teamAlpha, away: _teamCharlie, homeGoals: 0, awayGoals: allMatchesHaveGoals ? 2 : 0);
    }

    private static TournamentSettings CreateDefaultSettings()
    {
        return new TournamentSettings(
            id: DefaultSettingsId,
            startDate: DefaultStartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: 0,
            crossingsSeed: DefaultSeed,
            simulationSeed: 0,
            completionSeed: 0);
    }
}
