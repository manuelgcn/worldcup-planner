using Moq;
using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;
using ServiceTest.TestDoubles;
using ServiceTest.TestDoubles.Mocks;
using Match = ProjectCopa.Match;

namespace ServiceTest;

// Tests de caracterizacion para la Fase 9 (refactor GRASP/SOLID/Demeter).
//
// Objetivo: congelar el comportamiento ACTUAL antes del refactor, incluso cuando
// ese comportamiento es incorrecto. No "arreglan" nada; son una fotografia. Cuando
// el refactor cambie deliberadamente alguno de estos comportamientos (por ejemplo,
// que SimulateMatch pase a notificar a periodistas, o que el resultado 0-0 manual
// cuente como jugado), el test correspondiente debera actualizarse en su propio
// ciclo RED/GREEN, no en un commit de refactor puro.

[TestClass]
public sealed class Fase9NotifyResultLoadedCharacterizationTests
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
        _service = new NotificationService(_notificationRepository, _userRepository);
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
public sealed class Fase9MatchServiceResultFlowCharacterizationTests
{
    private const int MatchId = 1;

    private MatchServiceTestContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = new MatchServiceTestContext();
        _context.Service.CreateMatch(MatchServiceTestContext.NewGroupMatchRequest(MatchId));
    }

    // (b) Comportamiento invertido en Fase 9: SimulateMatch ahora SI notifica a periodistas.
    // MatchService pasa a colaborar con INotificationService (NotifyMatchSimulated), por lo
    // que un periodista registrado recibe la notificacion al simular un partido por el servicio.
    // Antes este test verificaba lo contrario (la UI/flujos agregados eran los unicos que avisaban).

    [TestMethod]
    public void SimulateMatch_NotifiesJournalists()
    {
        _context.UserRepository.Add(BuildJournalist(JournalistId: 1, "ana@mail.com"));

        _context.Service.SimulateMatch(MatchId);

        Assert.AreEqual(1, _context.NotificationRepository.GetAll().Count);
    }

    [TestMethod]
    public void SimulateMatch_AppliesRankingOnce()
    {
        _context.Service.SimulateMatch(MatchId);

        _context.RankingServiceMock.Verify(
            ranking => ranking.ApplyMatchResult(It.IsAny<Match>(), It.IsAny<MatchPhase>()),
            Times.Once);
    }

    // (c) SetGoals (el camino que usa hoy MatchResultEditor.razor) NO aplica ranking
    // y NO notifica. La UI hace esas dos cosas por fuera del servicio. Se contrasta
    // con SimulateMatch, que sobre el mismo contexto si aplica ranking.

    [TestMethod]
    public void SetGoals_DoesNotApplyRanking()
    {
        _context.Service.SetGoals(MatchId, homeGoals: 2, awayGoals: 1);

        _context.RankingServiceMock.Verify(
            ranking => ranking.ApplyMatchResult(It.IsAny<Match>(), It.IsAny<MatchPhase>()),
            Times.Never);
    }

    [TestMethod]
    public void SetGoals_DoesNotNotifyJournalists()
    {
        MockNotificationRepository notificationRepository = new MockNotificationRepository();
        MockUserRepository userRepository = new MockUserRepository();
        userRepository.Add(BuildJournalist(JournalistId: 1, "ana@mail.com"));
        _ = new NotificationService(notificationRepository, userRepository);

        _context.Service.SetGoals(MatchId, homeGoals: 2, awayGoals: 1);

        Assert.AreEqual(0, notificationRepository.GetAll().Count);
    }

    // (d) Congelamiento: una vez generado el knockout, editar un partido de grupos
    // (aca via SetGoals, el camino del editor manual) lanza MatchServicesException.

    [TestMethod]
    public void SetGoals_OnGroupMatch_WhenKnockoutGenerated_ThrowsMatchServicesException()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.SetGoals(MatchId, homeGoals: 2, awayGoals: 1));
    }

    private static User BuildJournalist(int JournalistId, string email)
    {
        User journalist = new User(
            JournalistId,
            "Ana",
            "Lopez",
            email,
            "Pass123@",
            new DateTime(1990, 5, 15));
        journalist.AddRole(UserRoles.Periodista);
        return journalist;
    }
}

[TestClass]
public sealed class Fase9GroupStandingsCharacterizationTests
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

    // (e) Bug conocido: la heuristica "partido jugado" es
    // (WasSimulated || HomeGoals + AwayGoals > 0). Un resultado MANUAL 0-0
    // (que no fue simulado) queda tratado como NO jugado: no suma partido jugado,
    // ni puntos de empate, y mantiene la fase de grupos como incompleta.

    [TestMethod]
    public void ManualZeroZero_IsCurrentlyTreatedAsNotPlayed_KnownBug()
    {
        AddManualMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 0, awayGoals: 0);

        List<GroupStandingDto> standings = _service.GetStandings(GroupName);

        Assert.IsTrue(standings.All(standing => standing.GamesPlayed == 0));
        Assert.IsTrue(standings.All(standing => standing.Points == 0));
    }

    [TestMethod]
    public void ManualZeroZero_DoesNotCompleteGroupStage_KnownBug()
    {
        AddManualMatch(matchId: 1, home: _teamAlpha, away: _teamBravo, homeGoals: 0, awayGoals: 0);

        Assert.IsFalse(_service.IsGroupStageComplete());
        Assert.AreEqual(1, _service.PendingGroupStageMatchCount());
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
