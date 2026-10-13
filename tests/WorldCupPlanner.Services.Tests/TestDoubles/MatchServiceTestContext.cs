using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using MockMatch = WorldCupPlanner.Domain.Tests.TestDoubles.MockMatch;

namespace WorldCupPlanner.Services.Tests.TestDoubles;

public sealed class MatchServiceTestContext
{
    
    private readonly Dictionary<int, Team> _teams = new();
    private readonly Dictionary<int, Stadium> _stadiums = new();
    private readonly Dictionary<string, Group> _groups = new();
    private readonly Dictionary<int, Journey> _journeys = new();
    private readonly Dictionary<int, TournamentSettings> _tournamentSettings = new();
    private readonly List<AuditLog> _auditLog = new();

    private ITeamRepository TeamRepository { get; set; } = null!;
    private IStadiumRepository StadiumRepository { get; set; } = null!;
    private IGroupRepository GroupRepository { get; set; } = null!;
    private IJourneyRepository JourneyRepository { get; set; } = null!;
    private ITournamentSettingsRepository TournamentSettingsRepository { get; set; } = null!;
    private IAuditLogRepository AuditLogRepository { get; set; } = null!;
    
    public MockMatchRepository MatchRepository { get; } = new MockMatchRepository();
    public MockEliminationMatchRepository EliminationMatchRepository { get; } = new MockEliminationMatchRepository();
    public MockMatchSimulationEngine SimulationEngine { get; } = new MockMatchSimulationEngine();
    public Mock<IRankingService> RankingServiceMock { get; } = new Mock<IRankingService>();
    public MockNotificationRepository NotificationRepository { get; } = new MockNotificationRepository();
    public MockUserRepository UserRepository { get; } = new MockUserRepository();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLog.AsReadOnly();

    private AuditLogService AuditLogService { get; set; } = null!;
    private NotificationService NotificationService { get; set; } = null!;
    
    public IMatchService Service { get; set; } = null!;
    private IMatchFactory MatchFactory { get; set; } = null!;

    public MatchServiceTestContext()
    {
        SeedData();
        BuildRepositories();
        BuildServices();
    }
    
    private void SeedData()
    {
        _teams[10] = new Team(10, "Home Team", 1500, false, Confederation.CONCACAF);
        _teams[20] = new Team(20, "Away Team", 1600, false, Confederation.UEFA);
        _stadiums[30] = new Stadium("Main Stadium", "Main City", "None", 25000);

        _groups["A"] = new Group("A");

        _journeys[1] = Journey.CreateInstance(1);

        TournamentSettings defaultSettings = new TournamentSettings(
            1,
            new DateTime(2026, 6, 1),
            3,
            3,
            123,
            456,
            789,
            999);

        _tournamentSettings[1] = defaultSettings;
        _tournamentSettings[60] = defaultSettings;
    }
    
    private void BuildRepositories()
    {
        TeamRepository = new SimpleTeamRepository(_teams);

        StadiumRepository = new SimpleStadiumRepository(_stadiums);

        GroupRepository = new SimpleGroupRepository(_groups);

        JourneyRepository = new SimpleJourneyRepository(_journeys);

        TournamentSettingsRepository = new SimpleTournamentSettingsRepository(_tournamentSettings);

        AuditLogRepository = new SimpleAuditLogRepository(_auditLog);
    }
    
    private void BuildServices()
    {
        AuditLogService = AuditLogService.CreateInstance(AuditLogRepository);

        // NotificationService real, respaldado por repos inspeccionables: permite verificar que
        // MatchService (no la UI) avisa a periodistas al simular/registrar resultados.
        NotificationService = new NotificationService(NotificationRepository, UserRepository, AuditLogService);

        // 1. Construimos la fábrica con sus responsabilidades de lectura, incluyendo IStadiumRepository
        MatchFactory = new MatchFactory(
            TeamRepository,
            StadiumRepository,
            GroupRepository,
            JourneyRepository,
            TournamentSettingsRepository,
            MatchRepository);

        // 2. Construimos el servicio core limpio, inyectando la fábrica y el log directamente
        Service = new MatchService(
            MatchRepository,
            TournamentSettingsRepository,
            MatchFactory,
            new SimulationEngineRegistry([SimulationEngine]),
            AuditLogService, // <-- Inyectado directamente en el servicio
            RankingServiceMock.Object,
            NotificationService,
            EliminationMatchRepository);
    }

    /// <summary>
    /// Helper used by Phase 6 guard tests: seeds an arbitrary MatchElimination
    /// into the elimination repository so MatchService.EnsureGroupMatchIsEditable
    /// trips its guard. The elimination match is a stub — only its presence
    /// in the repository matters.
    /// </summary>
    public void SeedFakeEliminationMatchInRepository()
    {
        Team home = _teams.Values.First();
        Team away = _teams.Values.Skip(1).FirstOrDefault() ?? home;
        Stadium stadium = _stadiums.Values.First();
        Group group = _groups.Values.First();
        Journey journey = _journeys.Values.First();
        DateTime sourceDate = new DateTime(2026, 6, 5, 14, 0, 0);
        DateTime elimDate = new DateTime(2026, 7, 1, 14, 0, 0);

        MockMatch sourceHome = new MockMatch(900, sourceDate, home, away, stadium);
        MockMatch sourceAway = new MockMatch(901, sourceDate, away, home, stadium);
        _ = group; _ = journey;

        MatchElimination elim = MatchElimination.Create(
            id: 9000,
            matchDate: elimDate,
            homeTeam: home,
            awayTeam: away,
            stadium: stadium,
            phase: Phase.RoundOf32,
            matchCode: "A1",
            matchHome: sourceHome,
            matchAway: sourceAway);
        EliminationMatchRepository.Add(elim);
    }

    // Simple concrete repository implementations
    private sealed class SimpleTeamRepository(Dictionary<int, Team> teams) : ITeamRepository
    {
        public void Add(Team team)
        {
            teams[team.Id] = team;
        }

        public void Update(Team team)
        {
            teams[team.Id] = team;
        }

        public void Delete(Team team)
        {
            teams.Remove(team.Id);
        }

        public Team? GetById(int teamId)
        {
            return teams.GetValueOrDefault(teamId);
        }

        public Team? GetByName(string name)
        {
            return teams.Values.FirstOrDefault(team => team.Name == name);
        }

        public bool ExistsByName(string name)
        {
            return teams.Values.Any(team => team.Name == name);
        }

        public IReadOnlyCollection<Team> GetAll()
        {
            return teams.Values.ToList();
        }

        public int CountByConfederation(Confederation confederation)
        {
            return teams.Values.Count(team => team.Confederation == confederation);
        }
    }

    private sealed class SimpleStadiumRepository(Dictionary<int, Stadium> stadiums) : IStadiumRepository
    {
        public void Add(Stadium stadium)
        {
            stadiums[stadium.Id] = stadium;
        }

        public void Update(Stadium stadium)
        {
            stadiums[stadium.Id] = stadium;
        }

        public void Delete(Stadium stadium)
        {
            stadiums.Remove(stadium.Id);
        }

        public Stadium? GetById(int stadiumId)
        {
            return stadiums.GetValueOrDefault(stadiumId);
        }

        public Stadium? GetByName(string name)
        {
            return stadiums.Values.FirstOrDefault(stadium => stadium.Name == name);
        }

        public bool ExistsByName(string name)
        {
            return stadiums.Values.Any(stadium => stadium.Name == name);
        }

        public IReadOnlyCollection<Stadium> GetAll()
        {
            return stadiums.Values.ToList();
        }
    }

    private sealed class SimpleGroupRepository(Dictionary<string, Group> groups) : IGroupRepository
    {
        public void Add(Group group)
        {
            groups[group.Name] = group;
        }

        public void Update(Group group)
        {
            groups[group.Name] = group;
        }

        public void Delete(Group group)
        {
            groups.Remove(group.Name);
        }

        public Group? GetByName(string name)
        {
            return groups.GetValueOrDefault(name);
        }

        public bool ExistsByName(string name)
        {
            return groups.ContainsKey(name);
        }

        public IReadOnlyCollection<Group> GetAll()
        {
            return groups.Values.ToList();
        }

        public void Clear()
        {
            groups.Clear();
        }
    }

    private sealed class SimpleJourneyRepository(Dictionary<int, Journey> journeys) : IJourneyRepository
    {
        public void Add(Journey journey)
        {
            journeys[journey.Number] = journey;
        }

        public void Update(Journey journey)
        {
            journeys[journey.Number] = journey;
        }

        public void Delete(Journey journey)
        {
            journeys.Remove(journey.Number);
        }

        public Journey? GetByNumber(int number)
        {
            return journeys.GetValueOrDefault(number);
        }

        public bool ExistsByNumber(int number)
        {
            return journeys.ContainsKey(number);
        }

        public IReadOnlyCollection<Journey> GetAll()
        {
            return journeys.Values.ToList();
        }

        public void Clear()
        {
            journeys.Clear();
        }
    }

    private sealed class SimpleTournamentSettingsRepository(Dictionary<int, TournamentSettings> settings) : ITournamentSettingsRepository
    {
        public void Add(TournamentSettings tournamentSettings)
        {
            settings[tournamentSettings.Id] = tournamentSettings;
        }

        public void Update(TournamentSettings tournamentSettings)
        {
            settings[tournamentSettings.Id] = tournamentSettings;
        }

        public void Delete(TournamentSettings tournamentSettings)
        {
            settings.Remove(tournamentSettings.Id);
        }

        public TournamentSettings? GetById(int tournamentSettingsId)
        {
            return settings.GetValueOrDefault(tournamentSettingsId);
        }

        public IReadOnlyCollection<TournamentSettings> GetAll()
        {
            return settings.Values.ToList();
        }
    }
    
    private sealed class SimpleAuditLogRepository(List<AuditLog> auditLogs) : IAuditLogRepository
    {
        public void Add(AuditLog auditLog)
        {
            int nextId = auditLogs.Count == 0 ? 1 : auditLogs.Max(a => a.Id) + 1;
            auditLog.SetId(nextId);
            auditLogs.Add(auditLog);
        }

        public List<AuditLog> GetAll()
        {
            return auditLogs;
        }

        public List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail)
        {
            return auditLogs
                .Where(log => !from.HasValue || log.Timestamp >= from.Value)
                .Where(log => !to.HasValue || log.Timestamp <= to.Value)
                .Where(log => string.IsNullOrWhiteSpace(action)
                    || log.Action.Contains(action.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(log => string.IsNullOrWhiteSpace(performedByEmail)
                    || (log.PerformedByEmail != null && log.PerformedByEmail.Contains(performedByEmail.Trim(), StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }

    public static CreateMatchRequestDto NewGroupMatchRequest(int id)
    {
        return new CreateMatchRequestDto
        {
            MatchId = id,
            MatchDate = new DateTime(2026, 6, 21),
            HomeTeamId = 10,
            AwayTeamId = 20,
            StadiumId = 30,
            GroupName = "A",
            JourneyId = 1
        };
    }

    public static CreateMatchRequestDto NewEliminationMatchRequest(int id = 2)
    {
        return new CreateMatchRequestDto
        {
            MatchId = id,
            MatchDate = new DateTime(2026, 6, 25),
            HomeTeamId = 10,
            AwayTeamId = 20,
            StadiumId = 30,
            IsElimination = true,
            TournamentSettingsId = 60,
            Phase = Phase.RoundOf16,
            MatchCode = "A1",
            MatchHomeId = 100,
            MatchAwayId = 200
        };
    }

    public void AddSourceEliminationMatches()
    {
        MatchRepository.Add(new MockMatch(
            100,
            new DateTime(2026, 6, 20),
            new Team(100, "Team 1", 1700, false, Confederation.CONCACAF),
            new Team(200, "Team 2", 1800, false, Confederation.UEFA),
            new Stadium("Stadium 1", "City 1", "None", 25000)));

        MatchRepository.Add(new MockMatch(
            200,
            new DateTime(2026, 6, 20),
            new Team(300, "Team 3", 1900, false, Confederation.CONCACAF),
            new Team(400, "Team 4", 2000, false, Confederation.UEFA),
            new Stadium("Stadium 2", "City 2", "None", 25000)));
    }
}
