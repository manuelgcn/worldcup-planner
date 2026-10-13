using WorldCupPlanner.Domain;
using WorldCupPlanner.DataAccess.Repositories;
using Moq;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class TournamentSimulationServiceTests
{
    private const int Seed = 42;
    private const int GroupCount = 12;
    private const int TeamsPerGroup = 4;
    private const int MatchesPerGroup = 6;
    private const int KnockoutMatchesCount = 16;
    private const int FirstGroupId = 1;
    private const int FirstMatchId = 1;
    private const int FirstTeamId = 1;
    private const int StartYear = 2026;
    private const int StartMonth = 6;
    private const int StartDay = 1;
    private const int GroupStageOffsetMatchDateDays = 0;
    private const int SecondMatchDateOffsetDays = 3;
    private const int ThirdMatchDateOffsetDays = 6;
    private const int HomeRankingBase = 2500;
    private const string AdminEmail = "admin@copa.com";

    private MockGroupRepository _groupRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private MockEliminationMatchRepository _eliminationMatchRepository = null!;
    private MockMatchSimulationEngine _matchSimulationEngine = null!;
    private MockKnockoutFixtureGenerator _knockoutFixtureGenerator = null!;
    private MockAuditLogRepository _auditLogRepository = null!;
    private MockTournamentSettingsRepository _tournamentSettingsRepository = null!;
    private Mock<IRankingService> _rankingServiceMock = null!;
    private MockNotificationService _notificationService = null!;
    private TournamentSimulationService _service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _groupRepository = new MockGroupRepository();
        _matchRepository = new MockMatchRepository();
        _eliminationMatchRepository = new MockEliminationMatchRepository();
        _matchSimulationEngine = new MockMatchSimulationEngine();
        _knockoutFixtureGenerator = new MockKnockoutFixtureGenerator();
        _auditLogRepository = new MockAuditLogRepository();
        _tournamentSettingsRepository = new MockTournamentSettingsRepository();
        _rankingServiceMock = new Mock<IRankingService>();

        SeedGroupsAndMatches();
        SeedSettings();
        _matchRepository.ResetCounters();
        _notificationService = new MockNotificationService();

        AuditLogService auditLogService = AuditLogService.CreateInstance(_auditLogRepository);
        _service = new TournamentSimulationService(
            _groupRepository,
            _matchRepository,
            _eliminationMatchRepository,
            new SimulationEngineRegistry([_matchSimulationEngine]),
            auditLogService,
            _tournamentSettingsRepository,
            _knockoutFixtureGenerator,
            notificationService: _notificationService,
            rankingService: _rankingServiceMock.Object);
    }

    [TestMethod]
    public void SimulateGroupStage_AppliesGroupStageRankingPerSimulatedMatch()
    {
        _service.SimulateGroupStage(Seed, AdminEmail);

        // Cada uno de los 72 partidos de grupos simulados debe actualizar el ranking como GroupStage
        _rankingServiceMock.Verify(
            r => r.ApplyMatchResult(It.IsAny<WorldCupPlanner.Domain.Match>(), MatchPhase.GroupStage),
            Times.Exactly(GroupCount * MatchesPerGroup));
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_SimulatesGroupMatchesAndPersistsKnockoutMatches()
    {
        _service.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);

        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchSimulationEngine.SimulateCalls);
        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchRepository.UpdateCalls);
        Assert.AreEqual(0, _matchRepository.AddCalls);
        Assert.AreEqual(KnockoutMatchesCount, _eliminationMatchRepository.GetAll().Count);
        Assert.IsNotNull(_knockoutFixtureGenerator.LastQualifiedTeamsReceived);
        Assert.IsTrue(_auditLogRepository.GetAll().Any(log => log.Action == AuditActions.SimulateGroupStageAndGenerateKnockout));
    }

    [TestMethod]
    public void SimulateGroupStage_SimulatesAllGroupMatchesWithoutGeneratingKnockout()
    {
        _service.SimulateGroupStage(Seed, AdminEmail);

        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchSimulationEngine.SimulateCalls);
        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchRepository.UpdateCalls);
        Assert.AreEqual(0, _matchRepository.AddCalls);
        Assert.AreEqual(0, _eliminationMatchRepository.GetAll().Count);
        Assert.AreEqual(Seed, _matchSimulationEngine.LastSeedReceived);
    }

    [TestMethod]
    public void SimulateGroupStage_KeepsResultsAlreadyLoadedByHand()
    {
        WorldCupPlanner.Domain.Match loaded = LoadResultByHand(homeGoals: 3, awayGoals: 0);

        _service.SimulateGroupStage(Seed, AdminEmail);

        AssertResultKept(loaded.MatchId, homeGoals: 3, awayGoals: 0);
        Assert.AreEqual(GroupCount * MatchesPerGroup - 1, _matchSimulationEngine.SimulateCalls);
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_KeepsResultsAlreadyLoadedByHand()
    {
        WorldCupPlanner.Domain.Match loaded = LoadResultByHand(homeGoals: 3, awayGoals: 0);

        _service.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);

        AssertResultKept(loaded.MatchId, homeGoals: 3, awayGoals: 0);
        Assert.AreEqual(GroupCount * MatchesPerGroup - 1, _matchSimulationEngine.SimulateCalls);
    }

    [TestMethod]
    public void SimulateGroupStage_LogsSimulateGroupStageActionWithSeedAndPerformer()
    {
        _service.SimulateGroupStage(Seed, AdminEmail);

        AuditLog log = _auditLogRepository.GetAll().First(entry => entry.Action == AuditActions.SimulateGroupStage);
        Assert.IsTrue(log.Details.Contains(Seed.ToString()), $"Expected seed in details, got '{log.Details}'");
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void SimulateGroupStage_NotifiesJournalistsOfAllMatchesInOneBatch()
    {
        _service.SimulateGroupStage(Seed, AdminEmail);

        Assert.AreEqual(1, _notificationService.SimulatedBatches.Count);
        Assert.AreEqual(GroupCount * MatchesPerGroup, _notificationService.SimulatedBatches[0].Count);
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_LogsSeedAndPerformer()
    {
        _service.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);

        AuditLog log = _auditLogRepository.GetAll().First(entry => entry.Action == AuditActions.SimulateGroupStageAndGenerateKnockout);
        Assert.IsTrue(log.Details.Contains($"seed {Seed}"), $"Expected seed in details, got '{log.Details}'");
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_WithoutGroupMatches_ThrowsMatchServicesException()
    {
        MockGroupRepository emptyGroupRepository = new MockGroupRepository();
        MockMatchRepository emptyMatchRepository = new MockMatchRepository();
        MockEliminationMatchRepository emptyEliminationRepository = new MockEliminationMatchRepository();
        MockKnockoutFixtureGenerator emptyKnockoutFixtureGenerator = new MockKnockoutFixtureGenerator();
        MockTournamentSettingsRepository emptySettingsRepository = new MockTournamentSettingsRepository();
        emptyGroupRepository.Seed(FirstGroupId, new Group("A"));
        emptySettingsRepository.Add(new TournamentSettings(1, new DateTime(StartYear, StartMonth, StartDay), 3, 3, Seed, Seed, Seed, Seed));

        AuditLogService auditLogService = AuditLogService.CreateInstance(_auditLogRepository);
        TournamentSimulationService service = new TournamentSimulationService(
            emptyGroupRepository,
            emptyMatchRepository,
            emptyEliminationRepository,
            new SimulationEngineRegistry([_matchSimulationEngine]),
            auditLogService,
            emptySettingsRepository,
            emptyKnockoutFixtureGenerator,
            new MockNotificationService(),
            _rankingServiceMock.Object);

        MatchServicesException exception = Assert.ThrowsException<MatchServicesException>(
            () => service.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail));

        Assert.AreEqual("El fixture aún no ha sido generado", exception.Message);
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_WhenGroupStageAlreadyPlayed_Throws()
    {
        // Todos los partidos de grupos quedan con resultado cargado: la fase ya se jugo.
        foreach (WorldCupPlanner.Domain.Match match in _matchRepository.GetAll())
        {
            match.SetGoals(1, 0);
        }

        MatchServicesException exception = Assert.ThrowsException<MatchServicesException>(
            () => _service.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail));

        Assert.AreEqual("La fase de grupos ya fue jugada", exception.Message);
        // No se simulo de nuevo ni se genero la fase 2.
        Assert.AreEqual(0, _matchSimulationEngine.SimulateCalls);
        Assert.AreEqual(0, _eliminationMatchRepository.GetAll().Count);
    }

    private void SeedSettings()
    {
        _tournamentSettingsRepository.Add(
            new TournamentSettings(
                1,
                new DateTime(StartYear, StartMonth, StartDay),
                3,
                3,
                Seed,
                Seed,
                Seed,
                Seed));
    }

    private void SeedGroupsAndMatches()
    {
        int teamId = FirstTeamId;
        int groupId = FirstGroupId;

        for (int groupIndex = 0; groupIndex < GroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);
            List<Team> teams = new List<Team>();

            for (int teamIndex = 0; teamIndex < TeamsPerGroup; teamIndex++)
            {
                Team team = new Team(teamId, "Team " + teamId, HomeRankingBase - teamId, false, Confederation.UEFA);
                teams.Add(team);
                group.AddTeam(team);
                teamId++;
            }

            _groupRepository.Seed(groupId, group);
            groupId++;

            AddGroupMatches(group, teams, groupIndex);
        }
    }

    private void AddGroupMatches(Group group, List<Team> teams, int groupIndex)
    {
        Stadium stadium = new Stadium("Stadium " + group.Name, "City", null, 30000);
        DateTime matchDate = new DateTime(StartYear, StartMonth, StartDay);
        int matchIdBase = (groupIndex * MatchesPerGroup) + FirstMatchId;

        AddMatch(matchIdBase + 0, matchDate.AddDays(GroupStageOffsetMatchDateDays), teams[0], teams[3], stadium, group, 1, "M" + (matchIdBase + 0));
        AddMatch(matchIdBase + 1, matchDate.AddDays(GroupStageOffsetMatchDateDays), teams[1], teams[2], stadium, group, 1, "M" + (matchIdBase + 1));
        AddMatch(matchIdBase + 2, matchDate.AddDays(SecondMatchDateOffsetDays), teams[0], teams[2], stadium, group, 2, "M" + (matchIdBase + 2));
        AddMatch(matchIdBase + 3, matchDate.AddDays(SecondMatchDateOffsetDays), teams[1], teams[3], stadium, group, 2, "M" + (matchIdBase + 3));
        AddMatch(matchIdBase + 4, matchDate.AddDays(ThirdMatchDateOffsetDays), teams[0], teams[1], stadium, group, 3, "M" + (matchIdBase + 4));
        AddMatch(matchIdBase + 5, matchDate.AddDays(ThirdMatchDateOffsetDays), teams[2], teams[3], stadium, group, 3, "M" + (matchIdBase + 5));
    }

    private void AddMatch(
        int matchId,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        Group group,
        int journeyNumber,
        string matchCode)
    {
        MatchGroup match = MatchGroup.CreateInstance(matchId, matchDate, homeTeam, awayTeam, stadium, group, Journey.CreateInstance(journeyNumber));
        match.SetMatchCode(matchCode);
        _matchRepository.Add(match);
    }

    private WorldCupPlanner.Domain.Match LoadResultByHand(int homeGoals, int awayGoals)
    {
        WorldCupPlanner.Domain.Match match = _matchRepository.GetAll().First();
        match.SetGoals(homeGoals, awayGoals);
        _matchRepository.Update(match);
        return match;
    }

    private void AssertResultKept(int matchId, int homeGoals, int awayGoals)
    {
        WorldCupPlanner.Domain.Match stored = _matchRepository.GetById(matchId)!;
        Assert.AreEqual(homeGoals, stored.HomeGoals);
        Assert.AreEqual(awayGoals, stored.AwayGoals);
    }
}
