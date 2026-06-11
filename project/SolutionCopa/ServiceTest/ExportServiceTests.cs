using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
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
        IAuditLogService auditService = AuditLogService.CreateInstance(_auditRepository, 1);
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
        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Details.Contains(journalist.email)));
    }

    [TestMethod]
    public void ExportAuditLogUsesDateRangeAndAuditsXlsxFile()
    {
        _auditRepository.Seed(AuditLog.Create("OLD", "Old", 1));
        Thread.Sleep(5);
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

    private static User BuildUser(UserRoles role)
    {
        User user = new User(1, "Test", "User", $"{role.ToString().ToLowerInvariant()}@copa.com", "Password@1", new DateTime(1990, 1, 1));
        user.AddRole(role);
        return user;
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day, 12, 0, 0, DateTimeKind.Utc);
}
