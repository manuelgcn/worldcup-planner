using Moq;
using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;
using ServiceTest.TestDoubles.Mocks;
using Match = ProjectCopa.Match;
using MockMatch = DomainTest.TestDoubles.MockMatch;

namespace ServiceTest;

// MatchService como controlador GRASP del caso de uso "registrar resultado": orquesta
// goles + ranking + auditoria + notificacion en una sola operacion, de modo que el
// comportamiento no dependa del punto de entrada (antes la UI lo hacia por su cuenta).
[TestClass]
public sealed class MatchServiceRegisterResultTests
{
    private const int MatchId = 1;
    private const string Email = "admin@copa.com";

    [TestMethod]
    public void RegisterResult_ReturnsDtoWithUpdatedGoals()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        MatchDto dto = harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);

        Assert.AreEqual(MatchId, dto.MatchId);
        Assert.AreEqual(2, dto.HomeGoals);
        Assert.AreEqual(1, dto.AwayGoals);
    }

    [TestMethod]
    public void RegisterResult_SetsGoalsOnStoredMatch()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.RegisterResult(MatchId, homeGoals: 3, awayGoals: 0, Email);

        Match stored = harness.MatchRepository.GetById(MatchId)!;
        Assert.AreEqual(3, stored.HomeGoals);
        Assert.AreEqual(0, stored.AwayGoals);
    }

    [TestMethod]
    public void RegisterResult_AppliesRankingOnce()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);

        harness.RankingMock.Verify(
            ranking => ranking.ApplyMatchResult(It.IsAny<Match>(), MatchPhase.GroupStage),
            Times.Once);
    }

    [TestMethod]
    public void RegisterResult_CalledTwice_AppliesRankingOnlyOnce()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);
        harness.Service.RegisterResult(MatchId, homeGoals: 3, awayGoals: 0, Email);

        harness.RankingMock.Verify(
            ranking => ranking.ApplyMatchResult(It.IsAny<Match>(), It.IsAny<MatchPhase>()),
            Times.Once);
    }

    [TestMethod]
    public void RegisterResult_AuditsWithActor()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);

        Assert.IsTrue(harness.AuditRepository.GetAll().Any(log => log.PerformedByEmail == Email));
    }

    // Replica EXACTAMENTE el comportamiento del editor actual: el servicio delega en
    // NotifyResultLoaded pasando el marcador previo; la decision de "solo si cambio"
    // sigue siendo responsabilidad de NotifyResultLoaded, no de RegisterResult.
    [TestMethod]
    public void RegisterResult_DelegatesToNotifyResultLoaded_WithPreviousGoals()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);

        Assert.AreEqual(1, harness.Notifications.ResultLoadedCalls.Count);
        MockNotificationService.ResultLoadedCall call = harness.Notifications.ResultLoadedCalls.Single();
        Assert.AreEqual(0, call.PreviousHomeGoals);
        Assert.AreEqual(0, call.PreviousAwayGoals);
        Assert.AreEqual(2, call.Match.HomeGoals);
        Assert.AreEqual(1, call.Match.AwayGoals);
    }

    [TestMethod]
    public void RegisterResult_UpdatesRepositoryAndCommits()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);
        int updates = harness.MatchRepository.UpdateCalls;
        int commits = harness.UnitOfWork.CommitCalls;

        harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email);

        Assert.AreEqual(updates + 1, harness.MatchRepository.UpdateCalls);
        Assert.AreEqual(commits + 1, harness.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void RegisterResult_WithNonexistentMatch_ThrowsMatchServicesException()
    {
        Harness harness = new Harness();

        Assert.ThrowsException<MatchServicesException>(
            () => harness.Service.RegisterResult(matchId: 999, homeGoals: 1, awayGoals: 0, Email));
    }

    [TestMethod]
    public void RegisterResult_OnGroupMatch_WhenKnockoutGenerated_ThrowsMatchServicesException()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);
        harness.SeedKnockout();

        Assert.ThrowsException<MatchServicesException>(
            () => harness.Service.RegisterResult(MatchId, homeGoals: 2, awayGoals: 1, Email));
    }

    // SimulateMatch ahora tambien notifica a periodistas (antes no colaboraba con
    // INotificationService). El motor del Harness simula 2-1.
    [TestMethod]
    public void SimulateMatch_NotifiesJournalistsViaNotifyMatchSimulated()
    {
        Harness harness = new Harness();
        harness.SeedGroupMatch(MatchId);

        harness.Service.SimulateMatch(MatchId);

        Assert.AreEqual(1, harness.Notifications.MatchSimulatedCalls.Count);
    }

    private sealed class Harness
    {
        public MockMatchRepository MatchRepository { get; } = new MockMatchRepository();
        public MockEliminationMatchRepository EliminationRepository { get; } = new MockEliminationMatchRepository();
        public MockNotificationService Notifications { get; } = new MockNotificationService();
        public Mock<IRankingService> RankingMock { get; } = new Mock<IRankingService>();
        public MockAuditLogRepository AuditRepository { get; } = new MockAuditLogRepository();
        public MockUnitOfWork UnitOfWork { get; } = new MockUnitOfWork();
        public MatchService Service { get; }

        private readonly MockMatchSimulationEngine _engine = new MockMatchSimulationEngine();
        private readonly MockTournamentSettingsRepository _settingsRepository = new MockTournamentSettingsRepository();

        public Harness()
        {
            _settingsRepository.Add(new TournamentSettings(
                id: 1,
                startDate: new DateTime(2026, 6, 1),
                maxMatchesPerDay: 3,
                daysBetweenJourneys: 3,
                fixtureSeed: 0,
                crossingsSeed: 0,
                simulationSeed: 0,
                completionSeed: 0));

            Service = new MatchService(
                MatchRepository,
                _settingsRepository,
                new StubMatchFactory(),
                new SimulationEngineRegistry(new IMatchSimulationEngine[] { _engine }),
                AuditLogService.CreateInstance(AuditRepository),
                RankingMock.Object,
                Notifications,
                UnitOfWork,
                EliminationRepository);
        }

        public void SeedGroupMatch(int id)
        {
            MatchRepository.Add(BuildGroupMatch(id));
        }

        public void SeedKnockout()
        {
            Team home = new Team(1, "Uruguay", 1500, false, Confederation.CONMEBOL);
            Team away = new Team(2, "Francia", 1600, false, Confederation.UEFA);
            Stadium stadium = new Stadium("Estadio Knockout", "Ciudad", null, 20000);
            EliminationRepository.Add(new MockMatch(9000, new DateTime(2026, 7, 1, 14, 0, 0), home, away, stadium));
        }

        private static MatchGroup BuildGroupMatch(int id)
        {
            Team home = new Team(1, "Uruguay", 1500, false, Confederation.CONMEBOL);
            Team away = new Team(2, "Francia", 1600, false, Confederation.UEFA);
            Stadium stadium = new Stadium("Estadio Test", "Ciudad", null, 20000);

            Group group = new Group("A");
            group.AddTeam(home);
            group.AddTeam(away);

            Journey journey = Journey.CreateInstance(1);

            return MatchGroup.CreateInstance(
                id: id,
                matchDate: new DateTime(2026, 6, 1, 14, 0, 0),
                homeTeam: home,
                awayTeam: away,
                stadium: stadium,
                group: group,
                journey: journey);
        }
    }

    // SimulateMatch y RegisterResult no pasan por la fabrica, pero el constructor la exige.
    private sealed class StubMatchFactory : IMatchFactory
    {
        public Match Create(CreateMatchRequestDto request)
        {
            throw new NotSupportedException();
        }

        public void ApplyUpdate(Match match, UpdateMatchRequestDto request)
        {
            throw new NotSupportedException();
        }
    }
}
