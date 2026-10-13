using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class FixtureViewServiceTests
{
    private MockMatchRepository _matchRepository = null!;
    private MockGroupRepository _groupRepository = null!;
    private MockTeamRepository _teamRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private IFixtureViewService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _matchRepository = new MockMatchRepository();
        _groupRepository = new MockGroupRepository();
        _teamRepository = new MockTeamRepository();
        _settingsRepository = new MockTournamentSettingsRepository();
        _service = new FixtureViewService(
            _matchRepository, _groupRepository, _teamRepository, _settingsRepository);
    }

    [TestMethod]
    public void GetSnapshot_ReturnsFirstSettingsFromRepository()
    {
        TournamentSettings settings = Settings();
        _settingsRepository.Add(settings);

        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.AreSame(settings, snapshot.Settings);
    }

    [TestMethod]
    public void GetSnapshot_WhenNoSettings_SettingsIsNull()
    {
        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.IsNull(snapshot.Settings);
    }

    [TestMethod]
    public void GetSnapshot_CountsTeams()
    {
        _teamRepository.Add(Team(1, "Uruguay"));
        _teamRepository.Add(Team(2, "Brasil"));

        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.AreEqual(2, snapshot.TeamCount);
    }

    [TestMethod]
    public void GetSnapshot_ReturnsGroupsOrderedByName()
    {
        _groupRepository.Add(new Group("C"));
        _groupRepository.Add(new Group("A"));
        _groupRepository.Add(new Group("B"));

        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        CollectionAssert.AreEqual(
            new[] { "A", "B", "C" },
            snapshot.Groups.Select(group => group.Name).ToArray());
    }

    [TestMethod]
    public void GetSnapshot_SeparatesGroupAndEliminationMatchesWithoutDuplication()
    {
        _matchRepository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "St1")));
        _matchRepository.Add(EliminationMatch(2, Date(20), Phase.RoundOf16, Stadium(1, "St1")));

        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.AreEqual(1, snapshot.GroupMatches.Count);
        Assert.AreEqual(1, snapshot.EliminationMatches.Count);
        Assert.AreEqual(1, snapshot.GroupMatches.Single().MatchId);
        Assert.AreEqual(2, snapshot.EliminationMatches.Single().MatchId);
    }

    [TestMethod]
    public void GetSnapshot_ComputesEarliestAndLatestMatchDate()
    {
        _matchRepository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "St1")));
        _matchRepository.Add(EliminationMatch(2, Date(25), Phase.RoundOf16, Stadium(1, "St1")));

        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.AreEqual(Date(10), snapshot.EarliestMatchDate);
        Assert.AreEqual(Date(25), snapshot.LatestMatchDate);
    }

    [TestMethod]
    public void GetSnapshot_WhenNoMatches_MatchDatesAreNull()
    {
        FixtureSnapshotDto snapshot = _service.GetSnapshot();

        Assert.IsNull(snapshot.EarliestMatchDate);
        Assert.IsNull(snapshot.LatestMatchDate);
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

    private static TournamentSettings Settings() =>
        new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 0, 0, 0);

    private static Team Team(int id, string name) =>
        new Team(id, name, 1000 + id, false, Confederation.CONMEBOL);

    private static Stadium Stadium(int id, string name) =>
        new Stadium(name, "Test City", null, 50000) { Id = id };

    private static MatchGroup GroupMatch(int id, DateTime date, string groupName, Stadium stadium)
    {
        MatchGroup match = MatchGroup.CreateInstance(
            id, date, Team(id * 10 + 1, "H" + id), Team(id * 10 + 2, "A" + id), stadium,
            new Group(groupName), Journey.CreateInstance(id));
        match.SetMatchCode("M" + id);
        return match;
    }

    private static MatchElimination EliminationMatch(int id, DateTime date, Phase phase, Stadium stadium)
    {
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026, 1, 1), 3, 3, 0, 0, 0, 0);
        return MatchElimination.CreateInitialRound(
            id, date, Team(id * 10 + 1, "H" + id), Team(id * 10 + 2, "A" + id), stadium, settings, phase, "E" + id);
    }
}
