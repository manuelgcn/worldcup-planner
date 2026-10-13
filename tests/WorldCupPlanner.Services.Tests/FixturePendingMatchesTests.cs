using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class FixturePendingMatchesTests
{
    private const int DefaultSettingsId = 1;
    private const int DefaultFixtureSeed = 7;
    private const int TotalExpectedTeams = 48;
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
    private ProbabilisticSimulationEngine _simulationEngine = null!;
    private GroupStandingsService _standingsService = null!;

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
        _simulationEngine = new ProbabilisticSimulationEngine();

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
            new SimulationEngineRegistry([_simulationEngine]));

        _standingsService = new GroupStandingsService(
            _matchRepository,
            _groupRepository,
            _settingsRepository);
    }

    [TestMethod]
    public void GenerateGroupStage_LeavesAllMatchesPending_NoGoalsNoWinnerNotSimulated()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        List<MatchGroup> groupMatches = _matchRepository.GetAll().OfType<MatchGroup>().ToList();

        Assert.AreEqual(72, groupMatches.Count, "Fixture should create 72 group matches.");
        foreach (MatchGroup match in groupMatches)
        {
            Assert.AreEqual(0, match.HomeGoals, $"MatchId={match.MatchId} should have 0 home goals after fixture generation.");
            Assert.AreEqual(0, match.AwayGoals, $"MatchId={match.MatchId} should have 0 away goals after fixture generation.");
            Assert.IsFalse(match.WasSimulated, $"MatchId={match.MatchId} must not be marked as simulated after fixture generation.");
            Assert.IsNull(match.GetWinner(), $"MatchId={match.MatchId} must not have a winner after fixture generation.");
        }
    }

    [TestMethod]
    public void GenerateGroupStage_DoesNotLogAnySimulationOrPublishAuditEntries()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        IReadOnlyCollection<AuditLog> logs = _auditRepository.GetAll();

        Assert.IsFalse(
            logs.Any(log => log.Action == AuditActions.SimulateMatch),
            "Fixture generation must not emit SIMULATE_MATCH audit entries.");
        Assert.IsFalse(
            logs.Any(log => log.Action == AuditActions.SimulationExecuted),
            "Fixture generation must not emit SIMULATION_EXECUTED audit entries.");
        Assert.IsFalse(
            logs.Any(log => log.Action == AuditActions.PublishMatchResult),
            "Fixture generation must not emit PUBLISH_MATCH_RESULT audit entries.");
        Assert.IsFalse(
            logs.Any(log => log.Action == AuditActions.SimulateRound),
            "Fixture generation must not emit SIMULATE_ROUND audit entries.");
        Assert.IsTrue(
            logs.Any(log => log.Action == AuditActions.GenerateFixture),
            "Fixture generation must emit GENERATE_FIXTURE audit entry.");
    }

    [TestMethod]
    public void GenerateGroupStage_AllStandingsShowZeroGamesPlayedAndZeroPoints()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        foreach (Group group in _groupRepository.GetAll())
        {
            List<GroupStandingDto> standings = _standingsService.GetStandings(group.Name);
            Assert.AreEqual(4, standings.Count, $"Group {group.Name} should have 4 standing rows.");
            foreach (GroupStandingDto row in standings)
            {
                Assert.AreEqual(0, row.GamesPlayed, $"Team {row.Team.Name} in group {group.Name} should have 0 games played before any simulation.");
                Assert.AreEqual(0, row.Wins, $"Team {row.Team.Name} should have 0 wins.");
                Assert.AreEqual(0, row.Draws, $"Team {row.Team.Name} should have 0 draws.");
                Assert.AreEqual(0, row.Losses, $"Team {row.Team.Name} should have 0 losses.");
                Assert.AreEqual(0, row.GoalsFor, $"Team {row.Team.Name} should have 0 goals for.");
                Assert.AreEqual(0, row.GoalsAgainst, $"Team {row.Team.Name} should have 0 goals against.");
                Assert.AreEqual(0, row.Points, $"Team {row.Team.Name} should have 0 points.");
            }
        }
    }

    [TestMethod]
    public void SimulatingOneMatch_OnlyAffectsItsTwoTeamsInStandings()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        TournamentSettings settings = _settingsRepository.GetById(DefaultSettingsId)!;

        MatchGroup chosen = _matchRepository.GetAll().OfType<MatchGroup>().First();
        string groupName = chosen.Group!.Name;
        int homeId = chosen.HomeTeam!.Id;
        int awayId = chosen.AwayTeam!.Id;

        _simulationEngine.Simulate(chosen, settings.SimulationSeed);

        List<GroupStandingDto> standings = _standingsService.GetStandings(groupName);
        foreach (GroupStandingDto row in standings)
        {
            bool teamPlayed = row.Team.Id == homeId || row.Team.Id == awayId;
            int expectedGames = teamPlayed ? 1 : 0;
            Assert.AreEqual(expectedGames, row.GamesPlayed, $"Team {row.Team.Name} should have {expectedGames} games played in group {groupName}.");
        }
    }

    [TestMethod]
    public void SimulatingOneMatch_LeavesAllOthersPending()
    {
        _orchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        TournamentSettings settings = _settingsRepository.GetById(DefaultSettingsId)!;

        List<MatchGroup> groupMatches = _matchRepository.GetAll().OfType<MatchGroup>().ToList();
        MatchGroup chosen = groupMatches.First();

        _simulationEngine.Simulate(chosen, settings.SimulationSeed);

        Assert.IsTrue(chosen.WasSimulated, "The explicitly simulated match should be marked as simulated.");

        List<MatchGroup> rest = groupMatches.Where(match => match.MatchId != chosen.MatchId).ToList();
        Assert.AreEqual(71, rest.Count);
        foreach (MatchGroup match in rest)
        {
            Assert.IsFalse(match.WasSimulated, $"Match {match.MatchId} must remain pending after simulating a different match.");
            Assert.AreEqual(0, match.HomeGoals, $"Match {match.MatchId} should still have 0 home goals.");
            Assert.AreEqual(0, match.AwayGoals, $"Match {match.MatchId} should still have 0 away goals.");
        }
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
            simulationSeed: 42,
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
            if (existing == null) return;
            _matches.Remove(existing);
            _matches.Add(match);
        }

        public void Delete(Match match) => _matches.RemoveAll(stored => stored.MatchId == match.MatchId);

        public bool ExistsById(int id) => _matches.Any(match => match.MatchId == id);

        public void Clear() => _matches.Clear();
    }
}
