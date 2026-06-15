using ProjectCopa;
using Services.Exceptions;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class TeamServicesTests
{
    private MockTeamRepository _repo = null!;
    private MockGroupRepository _groupRepo = null!;
    private MockUnitOfWork _uow = null!;
    private TeamServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new MockTeamRepository();
        _groupRepo = new MockGroupRepository();
        _uow = new MockUnitOfWork();
        _service = new TeamServices(_repo, _groupRepo, _uow, new MockAuditLogService());
    }

    [TestMethod]
    public void AddTeamCallsRepositoryAndCommits()
    {
        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);

        Assert.AreEqual(1, _repo.AddCalls);
        Assert.AreEqual(1, _uow.CommitCalls);
    }

    [TestMethod]
    public void CanAddTeam()
    {
        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);

        Assert.AreEqual(1, _service.GetTeams().Count);
    }

    [TestMethod]
    public void AddTeam_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        MockTeamRepository teamRepository = new MockTeamRepository();
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();

        TeamServices auditedService = new TeamServices(
            teamRepository,
            groupRepository,
            unitOfWork,
            AuditLogService.CreateInstance(repository));

        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        auditedService.AddTeam(team);

        List<AuditLog> logs = repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual(AuditActions.CreateTeam, logs[0].Action);
    }

    [TestMethod]
    public void RemoveTeam_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        MockTeamRepository teamRepository = new MockTeamRepository();
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockUnitOfWork unitOfWork = new MockUnitOfWork();

        TeamServices auditedService = new TeamServices(
            teamRepository,
            groupRepository,
            unitOfWork,
            AuditLogService.CreateInstance(repository));

        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        auditedService.AddTeam(team);
        auditedService.RemoveTeam("Uruguay");

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == AuditActions.DeleteTeam));
    }

    [TestMethod]
    public void AddTeam_RecordsPerformingActor()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        TeamServices auditedService = new TeamServices(
            new MockTeamRepository(),
            new MockGroupRepository(),
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(repository));

        auditedService.AddTeam(new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL), performedByEmail: "admin@copa.com");

        AuditLog log = repository.GetAll().Single(l => l.Action == AuditActions.CreateTeam);
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void RemoveTeam_RecordsPerformingActor()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        TeamServices auditedService = new TeamServices(
            new MockTeamRepository(),
            new MockGroupRepository(),
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(repository));

        auditedService.AddTeam(new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL));
        auditedService.RemoveTeam("Uruguay", performedByEmail: "admin@copa.com");

        AuditLog log = repository.GetAll().Single(l => l.Action == AuditActions.DeleteTeam);
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void CannotAddDuplicateTeamName()
    {
        Team t1 = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);
        Team t2 = new Team(2, "Uruguay", 1800, false, Confederation.CONMEBOL);

        _service.AddTeam(t1);

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(t2));
    }

    [TestMethod]
    public void CannotAddDuplicateTeamNameIgnoringCaseAndAccents()
    {
        _service.AddTeam(new Team(1, "Brasil", 2000, false, Confederation.CONMEBOL));

        Assert.ThrowsException<TeamServicesException>(
            () => _service.AddTeam(new Team(2, "BRASIL", 1800, false, Confederation.CONMEBOL)));
    }

    [TestMethod]
    public void CannotAddTeamWithEmptyName()
    {
        Assert.ThrowsException<ArgumentException>(() => new Team(1, "", 2000, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithLongName()
    {
        string longName = new string('A', 61);

        Assert.ThrowsException<ArgumentException>(() => new Team(1, longName, 2000, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithLowFifaRanking()
    {
        Assert.ThrowsException<ArgumentException>(() => new Team(1, "Uruguay", 299, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithHighFifaRanking()
    {
        Assert.ThrowsException<ArgumentException>(() => new Team(1, "Uruguay", 2501, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CanGetTeamByName()
    {
        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);

        Team found = _service.FindByName("Uruguay");

        Assert.AreEqual(team, found);
    }

    [TestMethod]
    public void GetTeamByNameThrowsIfNotFound()
    {
        Assert.ThrowsException<TeamServicesException>(() => _service.FindByName("NoExiste"));
    }

    [TestMethod]
    public void CanRemoveTeam()
    {
        Team team = new Team(1, "Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);
        _service.RemoveTeam("Uruguay");

        Assert.AreEqual(0, _service.GetTeams().Count);
    }

    [TestMethod]
    public void CannotRemoveNonExistingTeam()
    {
        Assert.ThrowsException<TeamServicesException>(() => _service.RemoveTeam("NoExiste"));
    }

    [TestMethod]
    public void CannotAddTeamBeyondUefaQuota()
    {
        for (int i = 1; i <= 16; i++)
        {
            _service.AddTeam(new Team(i, $"UEFA{i}", 1000 + i, false, Confederation.UEFA));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(17, "UEFA17", 1100, false, Confederation.UEFA)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondConmebolQuota()
    {
        for (int i = 1; i <= 7; i++)
        {
            _service.AddTeam(new Team(i, $"CONMEBOL{i}", 1400 + i, false, Confederation.CONMEBOL));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(8, "CONMEBOL8", 1500, false, Confederation.CONMEBOL)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondConcacafQuota()
    {
        for (int i = 1; i <= 7; i++)
        {
            _service.AddTeam(new Team(i, $"CONCACAF{i}", 1600 + i, false, Confederation.CONCACAF));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(8, "CONCACAF8", 1700, false, Confederation.CONCACAF)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondCafQuota()
    {
        for (int i = 1; i <= 9; i++)
        {
            _service.AddTeam(new Team(i, $"CAF{i}", 1800 + i, false, Confederation.CAF));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(10, "CAF10", 1900, false, Confederation.CAF)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondAfcQuota()
    {
        for (int i = 1; i <= 8; i++)
        {
            _service.AddTeam(new Team(i, $"AFC{i}", 2000 + i, false, Confederation.AFC));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(9, "AFC9", 2100, false, Confederation.AFC)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondOfcQuota()
    {
        _service.AddTeam(new Team(1, "OFC1", 2200, false, Confederation.OFC));

        Assert.ThrowsException<TeamServicesException>(() => _service.AddTeam(new Team(2, "OFC2", 2300, false, Confederation.OFC)));
    }

    [TestMethod]
    public void FixtureOrderingWithSameSeedProducesSameOrderWhenRankingHasTies()
    {
        List<Team> teams = new List<Team>();

        for (int i = 0; i < 48; i++)
        {
            int ranking = 2500 - (i / 4) * 10;
            Confederation conf = (Confederation)(i % 6);

            teams.Add(new Team(i + 1, $"Team{i + 1}", ranking, false, conf));
        }

        List<string> firstOrder = _service.OrderForFixture(teams, 2026).Select(t => t.Name).ToList();
        List<string> secondOrder = _service.OrderForFixture(teams, 2026).Select(t => t.Name).ToList();

        Assert.AreEqual(48, firstOrder.Count);
        CollectionAssert.AreEqual(firstOrder, secondOrder);
    }

    [TestMethod]
    public void OrderForFixtureThrowsTeamServicesExceptionWhenTeamCountIsNot48()
    {
        List<Team> teams = new List<Team>();

        for (int i = 0; i < 47; i++)
        {
            teams.Add(new Team(i + 1, $"Team{i + 1}", 2500 - i, false, Confederation.UEFA));
        }

        Assert.ThrowsException<TeamServicesException>(() => _service.OrderForFixture(teams, 2026));
    }

    [TestMethod]
    public void FillToFortyEight_FromEmpty_AddsExactlyFortyEightTeams()
    {
        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);

        Assert.AreEqual(48, _service.GetTeams().Count);
    }

    [TestMethod]
    public void FillToFortyEight_FromEmpty_RespectsConfederationQuotas()
    {
        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);

        IReadOnlyCollection<Team> teams = _service.GetTeams();
        Assert.AreEqual(16, teams.Count(team => team.Confederation == Confederation.UEFA));
        Assert.AreEqual(9, teams.Count(team => team.Confederation == Confederation.CAF));
        Assert.AreEqual(8, teams.Count(team => team.Confederation == Confederation.AFC));
        Assert.AreEqual(7, teams.Count(team => team.Confederation == Confederation.CONMEBOL));
        Assert.AreEqual(7, teams.Count(team => team.Confederation == Confederation.CONCACAF));
        Assert.AreEqual(1, teams.Count(team => team.Confederation == Confederation.OFC));
    }

    [TestMethod]
    public void FillToFortyEight_FromEmpty_GeneratedTeamsAreFictitious()
    {
        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);

        Assert.IsTrue(_service.GetTeams().All(team => team.IsFictitious));
    }

    [TestMethod]
    public void FillToFortyEight_WhenSomePresent_AddsOnlyMissingTeams()
    {
        _service.AddTeam(new Team(1, "Uruguay", 1800, false, Confederation.CONMEBOL));
        _service.AddTeam(new Team(2, "Brasil", 1900, false, Confederation.CONMEBOL));

        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);

        IReadOnlyCollection<Team> teams = _service.GetTeams();
        Assert.AreEqual(48, teams.Count);
        Assert.IsTrue(teams.Any(team => team.Name == "Uruguay" && !team.IsFictitious));
        Assert.IsTrue(teams.Any(team => team.Name == "Brasil" && !team.IsFictitious));
        Assert.AreEqual(7, teams.Count(team => team.Confederation == Confederation.CONMEBOL));
    }

    [TestMethod]
    public void FillToFortyEight_WhenAlreadyFortyEight_DoesNothing()
    {
        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);
        int countAfterFirst = _service.GetTeams().Count;

        _service.FillToFortyEight(completionSeed: 99, performedByEmail: null);

        Assert.AreEqual(countAfterFirst, _service.GetTeams().Count);
    }

    [TestMethod]
    public void FillToFortyEight_IsDeterministicForSameSeed()
    {
        _service.FillToFortyEight(completionSeed: 42, performedByEmail: null);
        List<int> firstRankings = _service.GetTeams().Select(team => team.FifaRanking).OrderBy(rank => rank).ToList();

        MockTeamRepository repo2 = new MockTeamRepository();
        MockGroupRepository groupRepo2 = new MockGroupRepository();
        MockUnitOfWork uow2 = new MockUnitOfWork();
        TeamServices service2 = new TeamServices(repo2, groupRepo2, uow2, new MockAuditLogService());

        service2.FillToFortyEight(completionSeed: 42, performedByEmail: null);
        List<int> secondRankings = service2.GetTeams().Select(team => team.FifaRanking).OrderBy(rank => rank).ToList();

        CollectionAssert.AreEqual(firstRankings, secondRankings);
    }

    [TestMethod]
    public void FillToFortyEight_GeneratesTeamsInFixedConfederationOrder()
    {
        // Blinda el orden de recorrido de confederaciones: no debe depender del orden
        // de iteracion de un Dictionary (que el lenguaje no garantiza). Recorriendo por
        // Id ascendente (orden de insercion) las confederaciones deben aparecer agrupadas
        // en el orden fijo AFC, CAF, OFC, UEFA, CONMEBOL, CONCACAF.
        _service.FillToFortyEight(completionSeed: 7, performedByEmail: null);

        List<Confederation> orderById = _service.GetTeams()
            .OrderBy(team => team.Id)
            .Select(team => team.Confederation)
            .ToList();

        List<Confederation> expected = new List<Confederation>();
        expected.AddRange(Enumerable.Repeat(Confederation.AFC, 8));
        expected.AddRange(Enumerable.Repeat(Confederation.CAF, 9));
        expected.AddRange(Enumerable.Repeat(Confederation.OFC, 1));
        expected.AddRange(Enumerable.Repeat(Confederation.UEFA, 16));
        expected.AddRange(Enumerable.Repeat(Confederation.CONMEBOL, 7));
        expected.AddRange(Enumerable.Repeat(Confederation.CONCACAF, 7));

        CollectionAssert.AreEqual(expected, orderById);
    }

    [TestMethod]
    public void FillToFortyEight_SameSeedProducesSameNamesAndRankingsPerId()
    {
        _service.FillToFortyEight(completionSeed: 42, performedByEmail: null);
        List<(int Id, string Name, int Ranking)> first = _service.GetTeams()
            .OrderBy(team => team.Id)
            .Select(team => (team.Id, team.Name, team.FifaRanking))
            .ToList();

        TeamServices service2 = new TeamServices(
            new MockTeamRepository(), new MockGroupRepository(), new MockUnitOfWork(), new MockAuditLogService());
        service2.FillToFortyEight(completionSeed: 42, performedByEmail: null);
        List<(int Id, string Name, int Ranking)> second = service2.GetTeams()
            .OrderBy(team => team.Id)
            .Select(team => (team.Id, team.Name, team.FifaRanking))
            .ToList();

        CollectionAssert.AreEqual(first, second);
    }

    [TestMethod]
    public void FillToFortyEight_AuditsCreateTeamPerGeneratedTeam()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        MockTeamRepository teamRepo = new MockTeamRepository();
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockUnitOfWork uow = new MockUnitOfWork();

        TeamServices auditedService = new TeamServices(
            teamRepo,
            groupRepository,
            uow,
            AuditLogService.CreateInstance(auditRepo));

        auditedService.FillToFortyEight(completionSeed: 7, performedByEmail: "admin@copa.com");

        int createLogs = auditRepo.GetAll().Count(log => log.Action == AuditActions.CreateTeam);
        Assert.AreEqual(48, createLogs);
    }
}