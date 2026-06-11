using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class ExportServiceTests
{
    private MockMatchRepository _matchRepository = null!;
    private MockAuditLogRepository _auditRepository = null!;
    private ExportService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _matchRepository = new MockMatchRepository();
        _auditRepository = new MockAuditLogRepository();
        IAuditLogService auditService = AuditLogService.CreateInstance(_auditRepository);
        _service = new ExportService(_matchRepository, _auditRepository, new Exporter(), auditService);
    }

    [TestMethod]
    public void ExportFixtureRejectsEditor()
    {
        User editor = BuildUser(UserRoles.Editor);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportFixture(new DateRange(Date(1), Date(30)), ExportFormat.Csv, editor));
    }

    [TestMethod]
    public void ExportAuditLogRejectsInvertedRange()
    {
        User admin = BuildUser(UserRoles.Admin);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportAuditLog(new DateRange(Date(30), Date(1)), ExportFormat.Csv, admin));
    }

    [TestMethod]
    public void ExportFixtureRejectsUnsupportedFormatWithoutAuditLog()
    {
        User admin = BuildUser(UserRoles.Admin);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportFixture(new DateRange(Date(1), Date(30)), (ExportFormat)999, admin));
        Assert.AreEqual(0, _auditRepository.GetAll().Count);
    }

    [TestMethod]
    public void ExportFixtureRejectedByRoleDoesNotCreateAuditLog()
    {
        User editor = BuildUser(UserRoles.Editor);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportFixture(new DateRange(Date(1), Date(30)), ExportFormat.Csv, editor));
        Assert.AreEqual(0, _auditRepository.GetAll().Count);
    }

    [TestMethod]
    public void ExportFixtureIncludesGroupAndEliminationMatchesAndAuditsFile()
    {
        Team home = new Team(1, "Uruguay", 1800, false, Confederation.CONMEBOL);
        Team away = new Team(2, "Argentina", 1900, false, Confederation.CONMEBOL);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);
        MatchGroup group = MatchGroup.CreateInstance(1, Date(10), home, away, stadium, new Group("A"), Journey.CreateInstance(1));
        MatchElimination elimination = MatchElimination.CreateInitialRound(
            2, Date(20), home, away, stadium, new TournamentSettings(), Phase.RoundOf32, "R32-1");
        _matchRepository.Add(group);
        _matchRepository.Add(elimination);
        User journalist = BuildUser(UserRoles.Periodista);

        ExportFileDto result = _service.ExportFixture(
            new DateRange(Date(1), Date(30)),
            ExportFormat.Csv,
            journalist);
        string csv = System.Text.Encoding.UTF8.GetString(result.Content);

        Assert.IsTrue(csv.Contains("Group"));
        Assert.IsTrue(csv.Contains("RoundOf32"));
        Assert.AreEqual("fixture_20260601_20260630.csv", result.FileName);
        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.ExportFixtureCsv));
        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.PerformedByEmail == journalist.email));
    }

    [TestMethod]
    public void ExportAuditLogUsesDateRangeAndAuditsXlsxFile()
    {
        _auditRepository.Seed(AuditLog.Create("OLD", "Old", 1));
        _auditRepository.Seed(AuditLog.Create("CURRENT", "Current", 1));
        User admin = BuildUser(UserRoles.Admin);
        DateTime today = DateTime.UtcNow.Date;

        ExportFileDto result = _service.ExportAuditLog(
            new DateRange(today, today),
            ExportFormat.Xlsx,
            admin);

        Assert.AreEqual("audit-log_" + today.ToString("yyyyMMdd") + "_" + today.ToString("yyyyMMdd") + ".xlsx", result.FileName);
        Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.ExportAuditXlsx));
    }

    [TestMethod]
    public void ExportAuditLogIncludesActorColumn()
    {
        _auditRepository.Seed(AuditLog.Create("ACTION", "Det", performedByEmail: "actor@copa.com"));
        User admin = BuildUser(UserRoles.Admin);
        DateTime today = DateTime.UtcNow.Date;

        ExportFileDto result = _service.ExportAuditLog(
            new DateRange(today, today),
            ExportFormat.Csv,
            admin);
        string csv = System.Text.Encoding.UTF8.GetString(result.Content);

        Assert.IsTrue(csv.Contains("PerformedByEmail"));
        Assert.IsTrue(csv.Contains("actor@copa.com"));
    }

    [TestMethod]
    public void ExportAuditLogAppliesActiveActionAndActorFilters()
    {
        _auditRepository.Seed(AuditLog.Create("CREATE_USER", "kept", performedByEmail: "admin@copa.com"));
        _auditRepository.Seed(AuditLog.Create("DELETE_TEAM", "wrong action", performedByEmail: "admin@copa.com"));
        _auditRepository.Seed(AuditLog.Create("CREATE_USER", "wrong actor", performedByEmail: "editor@copa.com"));
        User admin = BuildUser(UserRoles.Admin);
        DateTime today = DateTime.UtcNow.Date;

        ExportFileDto result = _service.ExportAuditLog(
            new DateRange(today, today),
            ExportFormat.Csv,
            admin,
            new AuditLogFilterDto { Action = "create", PerformedByEmail = "admin" });
        string csv = System.Text.Encoding.UTF8.GetString(result.Content);

        Assert.IsTrue(csv.Contains("kept"));
        Assert.IsFalse(csv.Contains("wrong action"));
        Assert.IsFalse(csv.Contains("wrong actor"));
    }

    [TestMethod]
    public void ExportRecordsRequesterAsStructuredActor()
    {
        User admin = BuildUser(UserRoles.Admin);
        DateTime today = DateTime.UtcNow.Date;

        _service.ExportAuditLog(new DateRange(today, today), ExportFormat.Csv, admin);

        AuditLog exportLog = _auditRepository.GetAll().Single(log => log.Action == AuditActions.ExportAuditCsv);
        Assert.AreEqual(admin.email, exportLog.PerformedByEmail);
        Assert.IsFalse(exportLog.Details.Contains(admin.email));
    }

    private static User BuildUser(UserRoles role)
    {
        User user = new User(1, "Test", "User", $"{role.ToString().ToLowerInvariant()}@copa.com", "Password@1", new DateTime(1990, 1, 1));
        user.AddRole(role);
        return user;
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day, 12, 0, 0, DateTimeKind.Utc);
}
