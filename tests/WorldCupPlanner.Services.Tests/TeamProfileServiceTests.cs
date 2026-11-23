using Microsoft.Extensions.DependencyInjection;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class TeamProfileServiceTests
{
    private const string GroupName = "A";
    private const int UnknownTeamId = 999;

    private static readonly Stadium Centenario = Stadium(1, "Centenario");
    private static readonly TournamentSettings Settings =
        new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 0, 0, 0);

    private MockTeamRepository _teamRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private ITeamProfileService _service = null!;
    private Group _group = null!;
    private Team _uruguay = null!;
    private Team _brasil = null!;
    private Team _chile = null!;
    private Team _peru = null!;
    private int _nextMatchId;

    [TestInitialize]
    public void Setup()
    {
        _teamRepository = new MockTeamRepository();
        _matchRepository = new MockMatchRepository();
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockTournamentSettingsRepository settingsRepository = new MockTournamentSettingsRepository();
        settingsRepository.Add(Settings);

        _group = new Group(GroupName);
        _uruguay = GroupTeam(1, "Uruguay");
        _brasil = GroupTeam(2, "Brasil");
        _chile = GroupTeam(3, "Chile");
        _peru = GroupTeam(4, "Peru");
        groupRepository.Add(_group);

        // La tabla sale del servicio real: el desempate no se reimplementa en el test.
        _service = new TeamProfileService(
            _teamRepository,
            _matchRepository,
            new GroupStandingsService(_matchRepository, groupRepository, settingsRepository));
        _nextMatchId = 1;
    }

    [TestMethod]
    public void GetProfile_WithAnUnknownId_ThrowsATeamServicesException()
    {
        TeamServicesException exception =
            Assert.ThrowsException<TeamServicesException>(() => _service.GetProfile(UnknownTeamId));

        Assert.AreEqual("No se encontró el equipo.", exception.Message);
    }

    [TestMethod]
    public void GetProfile_ReturnsTheTeamsNameFlagConfederationRankingAndWhetherItIsFictitious()
    {
        _teamRepository.Add(new Team(9, "Japon", 1650, true, Confederation.AFC, "AAAA"));

        TeamProfileDto profile = _service.GetProfile(9);

        Assert.AreEqual(9, profile.TeamId);
        Assert.AreEqual("Japon", profile.Name);
        Assert.AreEqual("AAAA", profile.FlagBase64);
        Assert.AreEqual(Confederation.AFC, profile.Confederation);
        Assert.AreEqual(1650, profile.FifaRanking);
        Assert.IsTrue(profile.IsFictitious);
    }

    [TestMethod]
    public void GetProfile_OfATeamWithoutMatches_HasNoGroupAndAnEmptyRecord()
    {
        _teamRepository.Add(new Team(9, "Japon", 1650, false, Confederation.AFC));

        TeamProfileDto profile = _service.GetProfile(9);

        Assert.IsNull(profile.GroupName);
        Assert.IsNull(profile.GroupPosition);
        Assert.AreEqual(0, profile.Played);
        Assert.AreEqual(0, profile.Points);
    }

    [TestMethod]
    public void GetProfile_ReportsTheGroupAndThePositionInItsStandings()
    {
        PlayGroupMatch(_brasil, 2, _uruguay, 1);
        PlayGroupMatch(_uruguay, 1, _chile, 0);
        PlayGroupMatch(_brasil, 1, _peru, 0);

        TeamProfileDto profile = _service.GetProfile(_uruguay.Id);

        Assert.AreEqual(GroupName, profile.GroupName);
        Assert.AreEqual(2, profile.GroupPosition);
    }

    [TestMethod]
    public void GetProfile_BeforeTheFirstResult_AlreadyReportsTheGroup()
    {
        _matchRepository.Add(GroupMatch(_uruguay, _brasil));

        TeamProfileDto profile = _service.GetProfile(_uruguay.Id);

        Assert.AreEqual(GroupName, profile.GroupName);
        Assert.IsNotNull(profile.GroupPosition);
        Assert.AreEqual(0, profile.Played);
    }

    [TestMethod]
    public void GetProfile_Record_CountsEveryMatchWithAResult_KnockoutsIncluded_AndAShootOutAsADraw()
    {
        PlayGroupMatch(_uruguay, 2, _brasil, 0);
        PlayGroupMatch(_chile, 1, _uruguay, 1);
        PlayGroupMatch(_uruguay, 0, _peru, 3);
        MatchElimination roundOf32 = KnockoutMatch(Phase.RoundOf32, _uruguay, Rival(10, "Mexico"), day: 28);
        roundOf32.SetGoals(1, 0);
        // Visitante y ganador por penales: para el record es un empate.
        MatchElimination roundOf16 = KnockoutMatch(Phase.RoundOf16, Rival(11, "Japon"), _uruguay, day: 30);
        roundOf16.SetGoals(2, 2);
        roundOf16.SetPenaltyShootoutResult(4, 5);
        // Sin resultado: todavia no cuenta.
        KnockoutMatch(Phase.QuarterFinals, _uruguay, Rival(12, "Francia"), day: 32);

        TeamProfileDto profile = _service.GetProfile(_uruguay.Id);

        Assert.AreEqual(5, profile.Played);
        Assert.AreEqual(2, profile.Won);
        Assert.AreEqual(2, profile.Drawn);
        Assert.AreEqual(1, profile.Lost);
        Assert.AreEqual(6, profile.GoalsFor);
        Assert.AreEqual(6, profile.GoalsAgainst);
        Assert.AreEqual(8, profile.Points);
    }

    [TestMethod]
    public void GetProfile_FindsTheTeamsMatchesById_EvenWhenTheMatchHoldsAnotherInstance()
    {
        // En produccion cada lectura del repositorio devuelve objetos nuevos.
        Team sameUruguay = new Team(_uruguay.Id, _uruguay.Name, 1500, false, Confederation.CONMEBOL);
        PlayGroupMatch(sameUruguay, 3, _brasil, 1);

        TeamProfileDto profile = _service.GetProfile(_uruguay.Id);

        Assert.AreEqual(1, profile.Played);
        Assert.AreEqual(1, profile.Won);
        Assert.AreEqual(1, profile.GroupPosition);
    }

    [TestMethod]
    public void AddWorldCupPlanner_RegistersTheTeamProfileService()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddWorldCupPlanner("Server=unused;Database=unused;", "flags");

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<TeamProfileService>(provider.GetService<ITeamProfileService>());
    }

    private Team GroupTeam(int id, string name)
    {
        Team team = new Team(id, name, 1500, false, Confederation.CONMEBOL);
        _teamRepository.Add(team);
        _group.AddTeam(team);
        return team;
    }

    private static Team Rival(int id, string name) => new Team(id, name, 1500, false, Confederation.UEFA);

    private MatchGroup PlayGroupMatch(Team home, int homeGoals, Team away, int awayGoals, int day = 10)
    {
        MatchGroup match = GroupMatch(home, away, day);
        match.SetGoals(homeGoals, awayGoals);
        _matchRepository.Add(match);
        return match;
    }

    private MatchGroup GroupMatch(Team home, Team away, int day = 10, Stadium? stadium = null)
    {
        int id = _nextMatchId++;
        MatchGroup match = MatchGroup.CreateInstance(
            id, Date(day), home, away, stadium ?? Centenario, _group, Journey.CreateInstance(1));
        match.SetMatchCode("M" + id);
        return match;
    }

    // Ya queda en el repositorio; el test le carga el resultado si lo necesita.
    private MatchElimination KnockoutMatch(Phase phase, Team home, Team away, int day)
    {
        int id = _nextMatchId++;
        MatchElimination match = MatchElimination.CreateInitialRound(
            id, Date(day), home, away, Centenario, Settings, phase, "E" + id);
        _matchRepository.Add(match);
        return match;
    }

    // Dias contados desde el 1 de junio de 2026: 32 es el 2 de julio.
    private static DateTime Date(int day) => new DateTime(2026, 6, 1).AddDays(day - 1);

    private static Stadium Stadium(int id, string name) =>
        new Stadium(name, "Test City", null, 50000) { Id = id };
}
