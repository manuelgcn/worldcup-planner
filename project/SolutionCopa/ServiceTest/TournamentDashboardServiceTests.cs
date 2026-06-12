using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Interfaces;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class TournamentDashboardServiceTests
{
    private MockMatchRepository _matchRepository = null!;
    private MockTeamRepository _teamRepository = null!;
    private MockStadiumRepository _stadiumRepository = null!;
    private MockFixtureOrchestrator _fixtureOrchestrator = null!;
    private ITournamentDashboardService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _matchRepository = new MockMatchRepository();
        _teamRepository = new MockTeamRepository();
        _stadiumRepository = new MockStadiumRepository();
        _fixtureOrchestrator = new MockFixtureOrchestrator();
        _service = new TournamentDashboardService(
            _matchRepository, _teamRepository, _stadiumRepository, _fixtureOrchestrator);
    }

    [TestMethod]
    public void GetOverview_CountsTeamsStadiumsAndMatches()
    {
        _teamRepository.Add(Team(1, "Uruguay"));
        _teamRepository.Add(Team(2, "Brasil"));
        _stadiumRepository.Add(Stadium(1, "Centenario"));
        _matchRepository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "Centenario")));

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.AreEqual(2, overview.TeamCount);
        Assert.AreEqual(1, overview.StadiumCount);
        Assert.AreEqual(1, overview.TotalMatches);
    }

    [TestMethod]
    public void GetOverview_CountsEachMatchExactlyOnce()
    {
        _matchRepository.Add(GroupMatch(1, Date(10), "A", Stadium(1, "St1")));
        _matchRepository.Add(EliminationMatch(2, Date(20), Phase.RoundOf16, Stadium(1, "St1")));

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.AreEqual(2, overview.TotalMatches);
    }

    [TestMethod]
    public void GetOverview_PlayedMatches_CountsSimulatedOrWithGoals()
    {
        MatchGroup withGoals = GroupMatch(1, Date(10), "A", Stadium(1, "St1"));
        withGoals.SetGoals(2, 1);
        _matchRepository.Add(withGoals);

        MatchElimination simulated = EliminationMatch(2, Date(20), Phase.RoundOf16, Stadium(1, "St1"));
        simulated.MarkAsSimulated();
        _matchRepository.Add(simulated);

        _matchRepository.Add(GroupMatch(3, Date(11), "A", Stadium(1, "St1")));

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.AreEqual(3, overview.TotalMatches);
        Assert.AreEqual(2, overview.PlayedMatches);
    }

    [TestMethod]
    public void GetOverview_ReadsPhaseFlagsFromOrchestrator()
    {
        _fixtureOrchestrator.FixtureGenerated = true;
        _fixtureOrchestrator.KnockoutGenerated = true;

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.IsTrue(overview.IsFixtureGenerated);
        Assert.IsTrue(overview.IsKnockoutGenerated);
    }

    [TestMethod]
    public void GetOverview_WhenFinalIsSimulated_ReturnsChampionName()
    {
        MatchElimination final = EliminationMatch(99, Date(30), Phase.Final, Stadium(1, "St1"));
        final.SetGoals(2, 1);
        final.MarkAsSimulated();
        _matchRepository.Add(final);

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.AreEqual("H99", overview.ChampionName);
    }

    [TestMethod]
    public void GetOverview_WhenFinalIsNotSimulated_ReturnsNullChampion()
    {
        MatchElimination final = EliminationMatch(99, Date(30), Phase.Final, Stadium(1, "St1"));
        final.SetGoals(2, 1);
        _matchRepository.Add(final);

        TournamentDashboardDto overview = _service.GetOverview();

        Assert.IsNull(overview.ChampionName);
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

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
