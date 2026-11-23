using Microsoft.Extensions.DependencyInjection;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class TournamentStatsServiceTests
{
    private static readonly Stadium Centenario = Stadium(1, "Centenario");

    private MockMatchRepository _matchRepository = null!;
    private ITournamentStatsService _service = null!;
    private int _nextMatchId;
    private int _nextTeamId;

    [TestInitialize]
    public void Setup()
    {
        _matchRepository = new MockMatchRepository();
        _service = new TournamentStatsService(_matchRepository);
        _nextMatchId = 1;
        _nextTeamId = 1;
    }

    [TestMethod]
    public void GetStats_WhenNoMatchHasAResult_ReturnsZerosAndNoHighlights()
    {
        _matchRepository.Add(GroupMatch(NewTeam("Uruguay"), NewTeam("Brasil")));

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(0, stats.MatchesPlayed);
        Assert.AreEqual(0, stats.Goals);
        Assert.AreEqual(0d, stats.GoalsPerMatch);
        Assert.AreEqual(0, stats.Draws);
        Assert.AreEqual(0, stats.PenaltyShootouts);
        Assert.AreEqual(0, stats.YellowCards);
        Assert.AreEqual(0, stats.RedCards);
        Assert.IsNull(stats.BiggestWin);
        Assert.IsNull(stats.HighestScoringMatch);
        Assert.AreEqual(0, stats.AttackLeaders.Count);
        Assert.AreEqual(0, stats.DefenceLeaders.Count);
    }

    [TestMethod]
    public void GetStats_CountsMatchesAndGoalsOnlyFromMatchesWithAResult()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 2, NewTeam("Brasil"), 1);
        PlayGroupMatch(NewTeam("Chile"), 0, NewTeam("Peru"), 0);
        _matchRepository.Add(GroupMatch(NewTeam("Ecuador"), NewTeam("Bolivia")));

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(2, stats.MatchesPlayed);
        Assert.AreEqual(3, stats.Goals);
        Assert.AreEqual(1.5, stats.GoalsPerMatch, 0.0001);
    }

    [TestMethod]
    public void GetStats_BiggestWin_IsTheWidestMargin_AndAmongEqualMarginsTheOneWithMoreGoals()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 3, NewTeam("Brasil"), 0);
        PlayGroupMatch(NewTeam("Chile"), 2, NewTeam("Peru"), 5);
        PlayGroupMatch(NewTeam("Ecuador"), 4, NewTeam("Bolivia"), 3);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(new MatchHighlightDto("Chile", "Peru", 2, 5), stats.BiggestWin);
    }

    [TestMethod]
    public void GetStats_BiggestWin_WithTheSameScore_IsTheEarliestMatch()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 3, NewTeam("Brasil"), 0, day: 12);
        PlayGroupMatch(NewTeam("Chile"), 3, NewTeam("Peru"), 0, day: 11);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(new MatchHighlightDto("Chile", "Peru", 3, 0), stats.BiggestWin);
    }

    [TestMethod]
    public void GetStats_HighestScoringMatch_IsTheOneWithMostGoals_AndAmongEqualsTheEarliest()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 3, NewTeam("Brasil"), 3, day: 12);
        PlayGroupMatch(NewTeam("Chile"), 4, NewTeam("Peru"), 2, day: 11);
        PlayGroupMatch(NewTeam("Ecuador"), 5, NewTeam("Bolivia"), 0, day: 10);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(new MatchHighlightDto("Chile", "Peru", 4, 2), stats.HighestScoringMatch);
    }

    [TestMethod]
    public void GetStats_WhenEveryMatchEndedGoalless_HasNoHighlights()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 0, NewTeam("Brasil"), 0);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(1, stats.MatchesPlayed);
        Assert.AreEqual(0d, stats.GoalsPerMatch);
        Assert.IsNull(stats.BiggestWin);
        Assert.IsNull(stats.HighestScoringMatch);
    }

    [TestMethod]
    public void GetStats_AMatchDecidedOnPenalties_CountsAsADrawAndAsAShootOut()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 2, NewTeam("Brasil"), 2);
        PlayGroupMatch(NewTeam("Chile"), 1, NewTeam("Peru"), 0);
        MatchElimination shootOut = KnockoutMatch(NewTeam("Ecuador"), NewTeam("Bolivia"));
        shootOut.SetGoals(1, 1);
        shootOut.SetPenaltyShootoutResult(4, 3);
        _matchRepository.Add(shootOut);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(2, stats.Draws);
        Assert.AreEqual(1, stats.PenaltyShootouts);
    }

    [TestMethod]
    public void GetStats_CountsYellowAndRedCardsOfMatchesWithAResult()
    {
        Team uruguay = NewTeam("Uruguay");
        Team brasil = NewTeam("Brasil");
        MatchGroup played = PlayGroupMatch(uruguay, 1, brasil, 0);
        played.AddIncident(new YellowCardIncident(1, 10, uruguay));
        played.AddIncident(new YellowCardIncident(2, 55, brasil));
        played.AddIncident(new RedCardIncident(3, 80, brasil));

        Team chile = NewTeam("Chile");
        MatchGroup pending = GroupMatch(chile, NewTeam("Peru"));
        pending.AddIncident(new YellowCardIncident(4, 5, chile));
        _matchRepository.Add(pending);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(2, stats.YellowCards);
        Assert.AreEqual(1, stats.RedCards);
    }

    [TestMethod]
    public void GetStats_TeamRows_AddUpEveryMatchOfATeam()
    {
        Team uruguay = NewTeam("Uruguay");
        // Otra instancia del mismo equipo, como cuando cada partido llega de una lectura distinta.
        Team uruguayAgain = new Team(uruguay.Id, "Uruguay", 1500, false, Confederation.CONMEBOL);
        PlayGroupMatch(uruguay, 3, NewTeam("Brasil"), 1);
        PlayGroupMatch(NewTeam("Chile"), 2, uruguayAgain, 2);

        TournamentStatsDto stats = _service.GetStats();

        Assert.AreEqual(3, stats.AttackLeaders.Count);
        Assert.AreEqual(
            new TeamGoalsDto(uruguay.Id, "Uruguay", Confederation.CONMEBOL, MatchesPlayed: 2, GoalsFor: 5, GoalsAgainst: 3),
            stats.AttackLeaders[0]);
    }

    [TestMethod]
    public void GetStats_AttackLeaders_AreOrderedByGoalsFor_AndTiesByNameIgnoringAccents()
    {
        PlayGroupMatch(NewTeam("Zambia"), 2, NewTeam("Éire"), 2);
        PlayGroupMatch(NewTeam("Egipto"), 2, NewTeam("Uruguay"), 4);

        TournamentStatsDto stats = _service.GetStats();

        CollectionAssert.AreEqual(
            new[] { "Uruguay", "Egipto", "Éire", "Zambia" },
            stats.AttackLeaders.Select(row => row.TeamName).ToArray());
    }

    [TestMethod]
    public void GetStats_Leaders_KeepOnlyTheTopFive()
    {
        PlayGroupMatch(NewTeam("Uruguay"), 6, NewTeam("Brasil"), 5);
        PlayGroupMatch(NewTeam("Chile"), 4, NewTeam("Peru"), 3);
        PlayGroupMatch(NewTeam("Ecuador"), 2, NewTeam("Bolivia"), 1);

        TournamentStatsDto stats = _service.GetStats();

        CollectionAssert.AreEqual(
            new[] { "Uruguay", "Brasil", "Chile", "Peru", "Ecuador" },
            stats.AttackLeaders.Select(row => row.TeamName).ToArray());
        CollectionAssert.AreEqual(
            new[] { "Ecuador", "Bolivia", "Chile", "Peru", "Uruguay" },
            stats.DefenceLeaders.Select(row => row.TeamName).ToArray());
    }

    [TestMethod]
    public void GetStats_DefenceLeaders_AreOrderedByFewestGoalsAgainst_ThenMostMatchesPlayed_ThenName()
    {
        Team uruguay = NewTeam("Uruguay");
        Team chile = NewTeam("Chile");
        PlayGroupMatch(uruguay, 1, NewTeam("Brasil"), 0);
        PlayGroupMatch(uruguay, 0, chile, 0);
        PlayGroupMatch(chile, 1, NewTeam("Peru"), 1);
        PlayGroupMatch(NewTeam("Ecuador"), 0, NewTeam("Bolivia"), 0);

        TournamentStatsDto stats = _service.GetStats();

        // Uruguay recibio 0 en dos partidos; Bolivia y Ecuador, 0 en uno; el resto recibio 1.
        CollectionAssert.AreEqual(
            new[] { "Uruguay", "Bolivia", "Ecuador", "Chile", "Brasil" },
            stats.DefenceLeaders.Select(row => row.TeamName).ToArray());
    }

    [TestMethod]
    public void AddWorldCupPlanner_RegistersTheStatsService()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddWorldCupPlanner("Server=unused;Database=unused;", "flags");

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<TournamentStatsService>(provider.GetService<ITournamentStatsService>());
    }

    private Team NewTeam(string name, Confederation confederation = Confederation.CONMEBOL) =>
        new Team(_nextTeamId++, name, 1500, false, confederation);

    private MatchGroup PlayGroupMatch(
        Team home, int homeGoals, Team away, int awayGoals, int day = 10, Stadium? stadium = null)
    {
        MatchGroup match = GroupMatch(home, away, day, stadium);
        match.SetGoals(homeGoals, awayGoals);
        _matchRepository.Add(match);
        return match;
    }

    private MatchGroup GroupMatch(Team home, Team away, int day = 10, Stadium? stadium = null)
    {
        int id = _nextMatchId++;
        MatchGroup match = MatchGroup.CreateInstance(
            id, Date(day), home, away, stadium ?? Centenario, new Group("A"), Journey.CreateInstance(1));
        match.SetMatchCode("M" + id);
        return match;
    }

    private MatchElimination KnockoutMatch(Team home, Team away)
    {
        int id = _nextMatchId++;
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026, 1, 1), 3, 3, 0, 0, 0, 0);
        return MatchElimination.CreateInitialRound(
            id, Date(28), home, away, Centenario, settings, Phase.RoundOf32, "E" + id);
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

    private static Stadium Stadium(int id, string name) =>
        new Stadium(name, "Test City", null, 50000) { Id = id };
}
