using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using Match = WorldCupPlanner.Domain.Match;

namespace WorldCupPlanner.Services.Tests;

// Tests de caracterizacion escritos antes del refactor GRASP/SOLID/Demeter.
//
// Objetivo: congelar el comportamiento ACTUAL antes del refactor, incluso cuando
// ese comportamiento es incorrecto. No "arreglan" nada; son una fotografia. Cuando
// el refactor cambie deliberadamente alguno de estos comportamientos (por ejemplo,
// que el resultado 0-0 manual cuente como jugado), el test correspondiente debera
// actualizarse en su propio
// ciclo RED/GREEN, no en un commit de refactor puro.

[TestClass]
public sealed class NotifyResultLoadedCharacterizationTests
{
    private const int JournalistId = 1;

    private MockNotificationRepository _notificationRepository = null!;
    private MockUserRepository _userRepository = null!;
    private NotificationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _notificationRepository = new MockNotificationRepository();
        _userRepository = new MockUserRepository();
        _userRepository.Add(BuildJournalist(JournalistId, "ana@mail.com"));
        _service = new NotificationService(_notificationRepository, _userRepository, new MockAuditLogService());
    }

    // (a) NotifyResultLoaded NO notifica siempre: solo cuando el marcador del partido
    // difiere del marcador previo recibido. Comparacion sobre AMBOS lados (local y visitante).

    [TestMethod]
    public void NotifyResultLoaded_WhenBothGoalsDifferFromPrevious_NotifiesJournalists()
    {
        Match match = BuildGroupMatch();
        match.SetGoals(2, 1);

        _service.NotifyResultLoaded(match, previousHomeGoals: 0, previousAwayGoals: 0);

        Assert.AreEqual(1, _notificationRepository.GetAll().Count);
    }

    [TestMethod]
    public void NotifyResultLoaded_WhenOnlyOneSideChanges_NotifiesJournalists()
    {
        Match match = BuildGroupMatch();
        match.SetGoals(2, 2);

        _service.NotifyResultLoaded(match, previousHomeGoals: 2, previousAwayGoals: 1);

        Assert.AreEqual(1, _notificationRepository.GetAll().Count);
    }

    [TestMethod]
    public void NotifyResultLoaded_WhenGoalsAreIdenticalToPrevious_DoesNotNotify()
    {
        Match match = BuildGroupMatch();
        match.SetGoals(2, 1);

        _service.NotifyResultLoaded(match, previousHomeGoals: 2, previousAwayGoals: 1);

        Assert.AreEqual(0, _notificationRepository.GetAll().Count);
    }

    [TestMethod]
    public void NotifyResultLoaded_WhenZeroZeroEqualsPreviousZeroZero_DoesNotNotify()
    {
        Match match = BuildGroupMatch();
        match.SetGoals(0, 0);

        _service.NotifyResultLoaded(match, previousHomeGoals: 0, previousAwayGoals: 0);

        Assert.AreEqual(0, _notificationRepository.GetAll().Count);
    }

    private static User BuildJournalist(int id, string email)
    {
        User journalist = new User(
            id,
            "Ana",
            "Lopez",
            email,
            "Pass123@",
            new DateTime(1990, 5, 15));
        journalist.AddRole(UserRoles.Periodista);
        return journalist;
    }

    private static MatchGroup BuildGroupMatch()
    {
        Team home = new Team(1, "Uruguay", 1500, false, Confederation.CONMEBOL);
        Team away = new Team(2, "Francia", 1600, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Estadio Test", "Ciudad", string.Empty, 20000);

        Group group = new Group("A");
        group.AddTeam(home);
        group.AddTeam(away);

        Journey journey = Journey.CreateInstance(1);

        return MatchGroup.CreateInstance(
            id: 100,
            matchDate: new DateTime(2026, 6, 1, 14, 0, 0),
            homeTeam: home,
            awayTeam: away,
            stadium: stadium,
            group: group,
            journey: journey);
    }
}

[TestClass]
public sealed class GroupStandingsCharacterizationTests
{
    private const string GroupName = "A";

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private MockMatchRepository _matchRepository = null!;
    private MockGroupRepository _groupRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private GroupStandingsService _service = null!;

    private Team _teamAlpha = null!;
    private Team _teamBravo = null!;
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
        _stadium = new Stadium("Centenario", "Montevideo", null, 60000) { Id = 1 };

        _group = new Group(GroupName);
        _group.AddTeam(_teamAlpha);
        _group.AddTeam(_teamBravo);
        _groupRepository.Add(_group);

        _service = new GroupStandingsService(_matchRepository, _groupRepository, _settingsRepository);
    }

    // (e) Bug corregido (WP5): un resultado MANUAL 0-0 ahora cuenta como jugado.
    // La fuente de verdad es Match.HasResult (se marca al cargar goles o simular), no la
    // heuristica de goles. Estos tests, antes etiquetados como bug conocido, ahora afirman
    // el comportamiento correcto.

    [TestMethod]
    public void ManualZeroZero_IsTreatedAsPlayed()
    {
        AddManualMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 0, awayGoals: 0);

        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.IsTrue(standings.All(standing => standing.GamesPlayed == 1));
        Assert.IsTrue(standings.All(standing => standing.Points == 1));
    }

    [TestMethod]
    public void ManualZeroZero_CompletesGroupStage()
    {
        AddManualMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 0, awayGoals: 0);

        Assert.IsTrue(_service.IsGroupStageComplete());
        Assert.AreEqual(0, _service.PendingGroupStageMatchCount());
    }

    private void AddManualMatch(int matchId, Team home, Team away, int homeGoals, int awayGoals)
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

    private static TournamentSettings CreateDefaultSettings()
    {
        return new TournamentSettings(
            id: 1,
            startDate: DefaultStartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: 0,
            crossingsSeed: 0,
            simulationSeed: 0,
            completionSeed: 0);
    }
}
