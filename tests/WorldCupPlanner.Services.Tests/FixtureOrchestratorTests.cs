using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class FixtureOrchestratorTests
{
    private const int DefaultSettingsId = 1;
    private const int DefaultFixtureSeed = 7;
    private const int TotalExpectedTeams = 48;
    private const int TotalExpectedGroups = 12;
    private const int TotalExpectedJourneys = 3;
    private const int TotalExpectedMatches = 72;
    private const int RequiredStadiumCount = 4;
    private const string AdminEmail = "admin@copa.com";

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private MockTeamRepository _teamRepository = null!;
    private MockStadiumRepository _stadiumRepository = null!;
    private MockGroupRepository _groupRepository = null!;
    private MockJourneyRepository _journeyRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private InMemoryEliminationMatchRepository _eliminationRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private MockAuditLogRepository _auditRepository = null!;
    private AuditLogService _auditService = null!;
    private FixtureOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void Setup()
    {
        _teamRepository = new MockTeamRepository();
        _stadiumRepository = new MockStadiumRepository();
        _groupRepository = new MockGroupRepository();
        _journeyRepository = new MockJourneyRepository();
        _matchRepository = new MockMatchRepository();
        _eliminationRepository = new InMemoryEliminationMatchRepository();
        _settingsRepository = new MockTournamentSettingsRepository();
        _auditRepository = new MockAuditLogRepository();
        _auditService = AuditLogService.CreateInstance(_auditRepository);

        _settingsRepository.Add(CreateDefaultSettings());
        SeedStadiums(RequiredStadiumCount);
        SeedTeams(TotalExpectedTeams);

        _orchestrator = new FixtureOrchestrator(
            _teamRepository,
            _stadiumRepository,
            _settingsRepository,
            _groupRepository,
            _journeyRepository,
            _matchRepository,
            _eliminationRepository,
            _auditService,
            new SimulationEngineRegistry([new ProbabilisticSimulationEngine(), new PureRandomSimulationEngine()]));
    }

    [TestMethod]
    public void GenerateGroupStage_WithLessThan48Teams_Throws()
    {
        _teamRepository = new MockTeamRepository();
        SeedTeams(47);
        _orchestrator = NewOrchestrator();

        Assert.ThrowsException<FixtureOrchestratorException>(
            () => _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateGroupStage_WithFewerThanFourStadiums_Throws()
    {
        _stadiumRepository = new MockStadiumRepository();
        SeedStadiums(3);
        _orchestrator = NewOrchestrator();

        Assert.ThrowsException<FixtureOrchestratorException>(
            () => _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateGroupStage_WithoutTournamentSettings_Throws()
    {
        _settingsRepository = new MockTournamentSettingsRepository();
        _orchestrator = NewOrchestrator();

        Assert.ThrowsException<FixtureOrchestratorException>(
            () => _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateGroupStage_Persists12Groups()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.AreEqual(TotalExpectedGroups, _groupRepository.GetAll().Count);
    }

    [TestMethod]
    public void GenerateGroupStage_PersistsGroupNamesAToL()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        List<string> expected = Enumerable.Range(0, TotalExpectedGroups)
            .Select(index => ((char)('A' + index)).ToString())
            .OrderBy(name => name)
            .ToList();
        List<string> actual = _groupRepository.GetAll()
            .Select(group => group.Name)
            .OrderBy(name => name)
            .ToList();
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GenerateGroupStage_PersistsThreeJourneys()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.AreEqual(TotalExpectedJourneys, _journeyRepository.GetAll().Count);
    }

    [TestMethod]
    public void GenerateGroupStage_PersistsSeventyTwoMatches()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.AreEqual(TotalExpectedMatches, _matchRepository.GetAll().Count);
    }

    [TestMethod]
    public void GenerateGroupStage_MatchesStartOnSettingsStartDate()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        DateTime earliestMatchDate = _matchRepository.GetAll().Min(match => match.MatchDate);
        Assert.AreEqual(DefaultStartDate.Date, earliestMatchDate.Date);
    }

    [TestMethod]
    public void GenerateGroupStage_MatchesUseTwoPmKickoffOrLater()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        TimeSpan minimumKickoff = new TimeSpan(14, 0, 0);
        Assert.IsTrue(_matchRepository.GetAll().All(match => match.MatchDate.TimeOfDay >= minimumKickoff));
    }

    [TestMethod]
    public void GenerateGroupStage_LogsAuditWithGenerateFixtureAction()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.GenerateFixture));
    }

    [TestMethod]
    public void GenerateGroupStage_AuditDetailsContainPerformedByEmail()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateFixture);
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void GenerateGroupStage_AuditDetailsMentionSeed()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: 99, selectedEngineName: null, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateFixture);
        Assert.IsTrue(log.Details.Contains("99"), $"Expected seed 99 in details, got '{log.Details}'");
    }

    [TestMethod]
    public void GenerateGroupStage_WhenAlreadyGenerated_Throws()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.ThrowsException<FixtureOrchestratorException>(
            () => _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void RegenerateGroupStage_WithoutKnockout_RebuildsFixture()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        _orchestrator.RegenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.AreEqual(TotalExpectedGroups, _groupRepository.GetAll().Count);
        Assert.AreEqual(TotalExpectedJourneys, _journeyRepository.GetAll().Count);
        Assert.AreEqual(TotalExpectedMatches, _matchRepository.GetAll().Count);
    }

    [TestMethod]
    public void RegenerateGroupStage_WithoutKnockout_LogsRegenerateAndClearActions()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        _auditRepository.GetAll().Clear();

        _orchestrator.RegenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.ClearFixture));
        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.RegenerateFixture));
    }

    [TestMethod]
    public void RegenerateGroupStage_WhenKnockoutAlreadyGenerated_Throws()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        SeedFakeEliminationMatch();

        Assert.ThrowsException<FixtureOrchestratorException>(
            () => _orchestrator.RegenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void DeleteGroupStage_RemovesAllGroupsJourneysAndMatches()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        _orchestrator.DeleteGroupStage(performedByEmail: AdminEmail);

        Assert.AreEqual(0, _groupRepository.GetAll().Count);
        Assert.AreEqual(0, _journeyRepository.GetAll().Count);
        Assert.AreEqual(0, _matchRepository.GetAll().Count);
    }

    [TestMethod]
    public void DeleteGroupStage_LogsDeleteFixtureActionWithPerformer()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        _auditRepository.GetAll().Clear();

        _orchestrator.DeleteGroupStage(performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.DeleteFixture);
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void DeleteGroupStage_WhenKnockoutGenerated_AlsoRemovesKnockout()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        SeedFakeEliminationMatch();

        _orchestrator.DeleteGroupStage(performedByEmail: AdminEmail);

        Assert.AreEqual(0, _groupRepository.GetAll().Count);
        Assert.AreEqual(0, _matchRepository.GetAll().Count);
        Assert.AreEqual(0, _eliminationRepository.GetAll().Count);
        Assert.IsFalse(_orchestrator.IsKnockoutGenerated());
    }

    [TestMethod]
    public void IsFixtureGenerated_BeforeGeneration_ReturnsFalse()
    {
        Assert.IsFalse(_orchestrator.IsFixtureGenerated());
    }

    [TestMethod]
    public void IsFixtureGenerated_AfterGeneration_ReturnsTrue()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_orchestrator.IsFixtureGenerated());
    }

    [TestMethod]
    public void IsKnockoutGenerated_BeforeKnockout_ReturnsFalse()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        Assert.IsFalse(_orchestrator.IsKnockoutGenerated());
    }

    [TestMethod]
    public void IsKnockoutGenerated_AfterEliminationMatchExists_ReturnsTrue()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        SeedFakeEliminationMatch();

        Assert.IsTrue(_orchestrator.IsKnockoutGenerated());
    }

    [TestMethod]
    public void GenerateGroupStage_WithSeedOverride_ProducesDeterministicAndDifferentResults()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: 11, selectedEngineName: null, performedByEmail: AdminEmail);
        List<string> firstFirstGroupTeams = _groupRepository.GetByName("A")!.Teams.Select(team => team.Name).ToList();

        _matchRepository.Clear();
        _groupRepository.Clear();
        _journeyRepository.Clear();
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: 22, selectedEngineName: null, performedByEmail: AdminEmail);
        List<string> secondFirstGroupTeams = _groupRepository.GetByName("A")!.Teams.Select(team => team.Name).ToList();

        bool atLeastOneDifference = firstFirstGroupTeams.Zip(secondFirstGroupTeams, (a, b) => a != b).Any(differs => differs);
        Assert.IsTrue(atLeastOneDifference, "Different seeds should produce at least one different team in group A");
    }

    private FixtureOrchestrator NewOrchestrator()
    {
        return new FixtureOrchestrator(
            _teamRepository,
            _stadiumRepository,
            _settingsRepository,
            _groupRepository,
            _journeyRepository,
            _matchRepository,
            _eliminationRepository,
            _auditService,
            new SimulationEngineRegistry([new ProbabilisticSimulationEngine(), new PureRandomSimulationEngine()]));
    }

    private static TournamentSettings CreateDefaultSettings()
    {
        return new TournamentSettings(
            id: DefaultSettingsId,
            startDate: DefaultStartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: DefaultFixtureSeed,
            crossingsSeed: 0,
            simulationSeed: 0,
            completionSeed: 0);
    }

    private void SeedStadiums(int count)
    {
        for (int index = 0; index < count; index++)
        {
            Stadium stadium = new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 };
            _stadiumRepository.Add(stadium);
        }
    }

    private void SeedTeams(int count)
    {
        Confederation[] confederations = Enum.GetValues<Confederation>();
        const int teamsPerRankingTier = 12;
        for (int teamIndex = 0; teamIndex < count; teamIndex++)
        {
            int tier = teamIndex / teamsPerRankingTier;
            Team team = new Team(
                id: teamIndex + 1,
                name: $"Team-{teamIndex + 1:D2}",
                fifaRanking: 2000 - (tier * 100),
                isFictitious: false,
                confederation: confederations[teamIndex % confederations.Length]);
            _teamRepository.Add(team);
        }
    }

    private void SeedFakeEliminationMatch()
    {
        Team home = _teamRepository.GetAll().First();
        Team away = _teamRepository.GetAll().Skip(1).First();
        Stadium stadium = _stadiumRepository.GetAll().First();
        Match anySource = _matchRepository.GetAll().First();
        Match anyOtherSource = _matchRepository.GetAll().Skip(1).First();
        MatchElimination elimination = MatchElimination.Create(
            id: 1000,
            matchDate: DefaultStartDate.AddDays(20),
            homeTeam: home,
            awayTeam: away,
            stadium: stadium,
            phase: Phase.RoundOf32,
            matchCode: "A1",
            matchHome: anySource,
            matchAway: anyOtherSource);
        _eliminationRepository.Add(elimination);
    }

    private sealed class InMemoryEliminationMatchRepository : IEliminationMatchRepository
    {
        private readonly List<Match> _matches = new();

        public Match? GetById(int id) => _matches.FirstOrDefault(match => match.MatchId == id);

        public IReadOnlyCollection<Match> GetByPhase(Phase phase) =>
            _matches.OfType<MatchElimination>().Where(match => match.Phase == phase).Cast<Match>().ToList();

        public IReadOnlyCollection<Match> GetAll() => _matches.AsReadOnly();

        public void Add(Match match) => _matches.Add(match);

        public void Update(Match match)
        {
            Match? existing = GetById(match.MatchId);
            if (existing == null)
            {
                return;
            }
            _matches.Remove(existing);
            _matches.Add(match);
        }

        public void Delete(Match match) => _matches.RemoveAll(stored => stored.MatchId == match.MatchId);

        public bool ExistsById(int id) => _matches.Any(match => match.MatchId == id);

        public void Clear() => _matches.Clear();
    }
}
