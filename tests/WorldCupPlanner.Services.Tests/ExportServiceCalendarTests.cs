using System.Text;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class ExportServiceCalendarTests
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
    public void ExportFixtureIcsRejectsEditorWithoutAuditLog()
    {
        User editor = BuildUser(UserRoles.Editor);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportFixtureIcs(new DateRange(Date(1), Date(25)), editor));
        Assert.AreEqual(0, _auditRepository.GetAll().Count);
    }

    [TestMethod]
    public void ExportFixtureIcsRejectsInvertedRange()
    {
        User admin = BuildUser(UserRoles.Admin);

        Assert.ThrowsException<ExportServiceException>(() =>
            _service.ExportFixtureIcs(new DateRange(Date(25), Date(1)), admin));
    }

    [TestMethod]
    public void ExportFixtureIcsWritesTheMatchesInRangeInDateOrderAndAuditsFile()
    {
        Team home = new Team(1, "Uruguay", 1800, false, Confederation.CONMEBOL);
        Team away = new Team(2, "Argentina", 1900, false, Confederation.CONMEBOL);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);
        _matchRepository.Add(MatchElimination.CreateInitialRound(
            2, Date(20), home, away, stadium, new TournamentSettings(), Phase.RoundOf32, "R32-1"));
        _matchRepository.Add(MatchGroup.CreateInstance(5, Date(10), home, away, stadium, new Group("A"), Journey.CreateInstance(1)));
        _matchRepository.Add(MatchGroup.CreateInstance(9, Date(28), home, away, stadium, new Group("A"), Journey.CreateInstance(2)));
        User journalist = BuildUser(UserRoles.Periodista);

        ExportFileDto result = _service.ExportFixtureIcs(new DateRange(Date(1), Date(25)), journalist);
        string calendar = Encoding.UTF8.GetString(result.Content);

        Assert.AreEqual("fixture_20260601_20260625.ics", result.FileName);
        Assert.AreEqual("text/calendar; charset=utf-8", result.ContentType);
        Assert.IsTrue(calendar.StartsWith("BEGIN:VCALENDAR\r\n"), "the file must start with the calendar, without a byte order mark");
        int groupEvent = calendar.IndexOf("UID:match-5@", StringComparison.Ordinal);
        int eliminationEvent = calendar.IndexOf("UID:match-2@", StringComparison.Ordinal);
        Assert.IsTrue(groupEvent >= 0 && groupEvent < eliminationEvent, "events must follow the match dates");
        Assert.IsFalse(calendar.Contains("UID:match-9@"));
        StringAssert.Contains(calendar, "SUMMARY:Uruguay vs Argentina\r\n");
        StringAssert.Contains(calendar, "LOCATION:Centenario\r\n");
        StringAssert.Contains(calendar, "DESCRIPTION:Grupo A\r\n");
        StringAssert.Contains(calendar, "DESCRIPTION:16avos de final\r\n");
        AuditLog exportLog = _auditRepository.GetAll().Single();
        Assert.AreEqual(AuditActions.ExportFixtureIcs, exportLog.Action);
        Assert.AreEqual(journalist.email, exportLog.PerformedByEmail);
    }

    private static User BuildUser(UserRoles role)
    {
        User user = new User(1, "Test", "User", $"{role.ToString().ToLowerInvariant()}@copa.com", "Password@1", new DateTime(1990, 1, 1));
        user.AddRole(role);
        return user;
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day, 12, 0, 0, DateTimeKind.Utc);
}
