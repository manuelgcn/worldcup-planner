using DataAccess.Repositories;
using ProjectCopa;
using Services;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class KnockoutFixtureGeneratorExtensionTests
{
    private const int DefaultSeed = 7;
    private const int DefaultSettingsId = 1;
    private const string AdminEmail = "admin@copa.com";
    private const int TotalGroupCount = 12;
    private const int TeamsPerGroup = 4;
    private const int MatchesPerGroup = 6;
    private const int ExpectedR32Matches = 16;
    private const int ExpectedR16Matches = 8;
    private const int ExpectedQuarterFinalMatches = 4;
    private const int ExpectedSemiFinalMatches = 2;
    private const int ExpectedFinalMatches = 1;
    private const int ExpectedThirdPlaceMatches = 1;

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private MockGroupRepository _groupRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private MockEliminationMatchRepository _eliminationRepository = null!;
    private MockStadiumRepository _stadiumRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private MockMatchSimulationEngine _simulationEngine = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private MockAuditLogRepository _auditRepository = null!;
    private AuditLogService _auditService = null!;
    private KnockoutFixtureGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _groupRepository = new MockGroupRepository();
        _matchRepository = new MockMatchRepository();
        _eliminationRepository = new MockEliminationMatchRepository();
        _stadiumRepository = new MockStadiumRepository();
        _settingsRepository = new MockTournamentSettingsRepository();
        _simulationEngine = new MockMatchSimulationEngine();
        _unitOfWork = new MockUnitOfWork();
        _auditRepository = new MockAuditLogRepository();
        _auditService = AuditLogService.CreateInstance(_auditRepository);

        SeedStadiums();
        SeedSettings();
        SeedGroups();

        _generator = NewGenerator();
    }

    [TestMethod]
    public void IsBracketGenerated_BeforeGenerateBracket_ReturnsFalse()
    {
        SeedPlayedGroupStageMatches();

        Assert.IsFalse(_generator.IsBracketGenerated());
    }

    [TestMethod]
    public void IsBracketGenerated_AfterGenerateBracket_ReturnsTrue()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_generator.IsBracketGenerated());
    }

    [TestMethod]
    public void GenerateBracket_PersistsSixteenR32MatchesInEliminationRepo()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        IReadOnlyCollection<Match> elimMatches = _eliminationRepository.GetAll();
        Assert.AreEqual(ExpectedR32Matches, elimMatches.Count);
        Assert.IsTrue(elimMatches.All(match => match is MatchElimination elimination && elimination.Phase == Phase.RoundOf32));
    }

    [TestMethod]
    public void GenerateBracket_AuditsGenerateKnockoutAction()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.GenerateKnockout));
    }

    [TestMethod]
    public void GenerateBracket_AuditDetailsContainPerformedByEmail()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateKnockout);
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void GenerateBracket_AuditDetailsContainSeed()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: 42, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateKnockout);
        Assert.IsTrue(log.Details.Contains("42"), $"Expected seed 42 in details, got '{log.Details}'");
    }

    [TestMethod]
    public void GenerateBracket_WhenAlreadyGenerated_Throws()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateBracket_WhenGroupStageNotComplete_Throws()
    {
        SeedUnplayedGroupStageMatches();

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateBracket_CallsUnitOfWorkCommit()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_unitOfWork.CommitCalls >= 1);
    }

    [TestMethod]
    public void SimulateNextRound_SimulatesR32MatchesWhenR32IsCurrentRound()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        Assert.AreEqual(ExpectedR32Matches, _simulationEngine.SimulateCalls);
    }

    [TestMethod]
    public void SimulateNextRound_CreatesR16PhaseAfterR32Simulated()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        IReadOnlyCollection<Match> elim = _eliminationRepository.GetAll();
        Assert.AreEqual(ExpectedR16Matches, elim.OfType<MatchElimination>().Count(match => match.Phase == Phase.RoundOf16));
    }

    [TestMethod]
    public void SimulateNextRound_AuditsSimulateRoundAction()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.SimulateRound));
    }

    [TestMethod]
    public void SimulateUntilFinal_LeavesAllRoundsCompleteIncludingThirdPlace()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateUntilFinal(performedByEmail: AdminEmail);

        List<MatchElimination> all = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        Assert.AreEqual(ExpectedR32Matches, all.Count(match => match.Phase == Phase.RoundOf32));
        Assert.AreEqual(ExpectedR16Matches, all.Count(match => match.Phase == Phase.RoundOf16));
        Assert.AreEqual(ExpectedQuarterFinalMatches, all.Count(match => match.Phase == Phase.QuarterFinals));
        Assert.AreEqual(ExpectedSemiFinalMatches, all.Count(match => match.Phase == Phase.SemiFinal));
        Assert.AreEqual(ExpectedFinalMatches, all.Count(match => match.Phase == Phase.Final));
        Assert.AreEqual(ExpectedThirdPlaceMatches, all.Count(match => match.Phase == Phase.ThirdPlace));
    }

    [TestMethod]
    public void SimulateUntilFinal_AuditsMultipleSimulateRoundActions()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateUntilFinal(performedByEmail: AdminEmail);

        int simulateRoundLogs = _auditRepository.GetAll().Count(log => log.Action == AuditActions.SimulateRound);
        Assert.IsTrue(simulateRoundLogs >= 5, $"Expected at least 5 SimulateRound logs (R32, R16, QF, SF, Final/3PL), got {simulateRoundLogs}");
    }

    private KnockoutFixtureGenerator NewGenerator()
    {
        return new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([_simulationEngine]),
            _matchRepository,
            _eliminationRepository,
            _groupRepository,
            _stadiumRepository,
            _settingsRepository,
            _unitOfWork,
            _auditService,
            new MockNotificationService(),
            new MockRankingService());
    }

    private void SeedStadiums()
    {
        for (int index = 0; index < 4; index++)
        {
            Stadium stadium = new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 };
            _stadiumRepository.Add(stadium);
        }
    }

    private void SeedSettings()
    {
        _settingsRepository.Add(new TournamentSettings(
            id: DefaultSettingsId,
            startDate: DefaultStartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: DefaultSeed,
            crossingsSeed: DefaultSeed,
            simulationSeed: DefaultSeed,
            completionSeed: DefaultSeed));
    }

    private void SeedGroups()
    {
        int teamId = 1;
        for (int groupIndex = 0; groupIndex < TotalGroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);
            for (int teamIndex = 0; teamIndex < TeamsPerGroup; teamIndex++)
            {
                Team team = new Team(teamId, $"T{teamId:D2}", 2500 - teamId, false, Confederation.UEFA);
                group.AddTeam(team);
                teamId++;
            }
            _groupRepository.Add(group);
        }
    }

    private void SeedPlayedGroupStageMatches()
    {
        AddGroupStageMatches(homeGoals: 2, awayGoals: 1);
    }

    // Sin resultado registrado: los partidos quedan sin goles ni simulacion, por lo que
    // Match.HasResult es false y la fase de grupos sigue incompleta.
    private void SeedUnplayedGroupStageMatches()
    {
        AddGroupStageMatches(homeGoals: null, awayGoals: null);
    }

    private void AddGroupStageMatches(int? homeGoals, int? awayGoals)
    {
        Stadium stadium = _stadiumRepository.GetAll().First();
        int matchId = 1;
        foreach (Group group in _groupRepository.GetAll())
        {
            List<Team> teams = group.Teams.ToList();
            (int home, int away)[] pairs =
            {
                (0, 3), (1, 2),
                (0, 2), (1, 3),
                (0, 1), (2, 3)
            };

            int journeyIndex = 0;
            for (int matchIndex = 0; matchIndex < MatchesPerGroup; matchIndex++)
            {
                int journeyNumber = (matchIndex / 2) + 1;
                Journey journey = Journey.CreateInstance(journeyNumber);
                (int home, int away) pair = pairs[matchIndex];
                MatchGroup match = MatchGroup.CreateInstance(
                    id: matchId++,
                    matchDate: DefaultStartDate.AddDays(journeyNumber * 3),
                    homeTeam: teams[pair.home],
                    awayTeam: teams[pair.away],
                    stadium: stadium,
                    group: group,
                    journey: journey);
                match.SetMatchCode($"M{match.MatchId}");
                if (homeGoals.HasValue && awayGoals.HasValue)
                {
                    match.SetGoals(homeGoals.Value, awayGoals.Value);
                }
                _matchRepository.Add(match);
                journeyIndex++;
            }
        }
    }
}
