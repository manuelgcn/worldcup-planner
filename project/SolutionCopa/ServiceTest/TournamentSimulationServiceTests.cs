using ProjectCopa;
using DataAccess.Repositories;
using Services;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

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
    private MockUnitOfWork _unitOfWork = null!;
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
        _unitOfWork = new MockUnitOfWork();

        SeedGroupsAndMatches();
        SeedSettings();
        _matchRepository.ResetCounters();

        AuditLogService auditLogService = AuditLogService.CreateInstance(_auditLogRepository);
        _service = new TournamentSimulationService(
            _groupRepository,
            _matchRepository,
            _eliminationMatchRepository,
            _unitOfWork,
            new SimulationEngineRegistry([_matchSimulationEngine]),
            auditLogService,
            _tournamentSettingsRepository,
            _knockoutFixtureGenerator);
    }

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_SimulatesGroupMatchesAndPersistsKnockoutMatches()
    {
        _service.SimulateGroupStageAndGenerateKnockout(Seed);

        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchSimulationEngine.SimulateCalls);
        Assert.AreEqual(GroupCount * MatchesPerGroup, _matchRepository.UpdateCalls);
        Assert.AreEqual(0, _matchRepository.AddCalls);
        Assert.AreEqual(KnockoutMatchesCount, _eliminationMatchRepository.GetAll().Count);
        Assert.IsNotNull(_knockoutFixtureGenerator.LastQualifiedTeamsReceived);
        Assert.AreEqual(1, _unitOfWork.CommitCalls);
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
        Assert.IsTrue(_unitOfWork.CommitCalls >= 1);
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
    public void SimulateGroupStageAndGenerateKnockout_WithoutGroupMatches_ThrowsMatchServicesException()
    {
        MockGroupRepository emptyGroupRepository = new MockGroupRepository();
        MockMatchRepository emptyMatchRepository = new MockMatchRepository();
        MockEliminationMatchRepository emptyEliminationRepository = new MockEliminationMatchRepository();
        MockKnockoutFixtureGenerator emptyKnockoutFixtureGenerator = new MockKnockoutFixtureGenerator();
        MockTournamentSettingsRepository emptySettingsRepository = new MockTournamentSettingsRepository();
        MockUnitOfWork emptyUnitOfWork = new MockUnitOfWork();
        emptyGroupRepository.Seed(FirstGroupId, new Group("A"));
        emptySettingsRepository.Add(new TournamentSettings(1, new DateTime(StartYear, StartMonth, StartDay), 3, 3, Seed, Seed, Seed, Seed));

        AuditLogService auditLogService = AuditLogService.CreateInstance(_auditLogRepository);
        TournamentSimulationService service = new TournamentSimulationService(
            emptyGroupRepository,
            emptyMatchRepository,
            emptyEliminationRepository,
            emptyUnitOfWork,
            new SimulationEngineRegistry([_matchSimulationEngine]),
            auditLogService,
            emptySettingsRepository,
            emptyKnockoutFixtureGenerator);

        MatchServicesException exception = Assert.ThrowsException<MatchServicesException>(
            () => service.SimulateGroupStageAndGenerateKnockout(Seed));

        Assert.AreEqual("El fixture aún no ha sido generado", exception.Message);
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

}
