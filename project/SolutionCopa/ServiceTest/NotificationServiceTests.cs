using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class NotificationServiceTests
{
    private const int JournalistId = 1;
    private const int OtherUserId = 99;

    private MockNotificationRepository notificationRepository = null!;
    private MockUserRepository userRepository = null!;
    private NotificationService service = null!;

    [TestInitialize]
    public void Setup()
    {
        notificationRepository = new MockNotificationRepository();
        userRepository = new MockUserRepository();
        service = new NotificationService(notificationRepository, userRepository);
    }

    [TestMethod]
    public void NotifyJournalists()
    {
        User journalist1 = BuildJournalist(1, "ana@mail.com");
        User journalist2 = BuildJournalist(2, "luis@mail.com");

        User admin = new User(
            3,
            "Admin",
            "Sistema",
            "admin@mail.com",
            "Pass123@",
            new DateTime(1985, 1, 1)
        );
        admin.AddRole(UserRoles.Admin);

        userRepository.Add(journalist1);
        userRepository.Add(journalist2);
        userRepository.Add(admin);

        service.NotifyJournalists("Resultado cargado");

        IReadOnlyCollection<Notification> stored = notificationRepository.GetAll();

        Assert.AreEqual(2, stored.Count);
        Assert.IsTrue(stored.All(n => !n.IsRead));
        Assert.IsTrue(stored.Any(n => n.UserId == 1 && n.Message == "Resultado cargado"));
        Assert.IsTrue(stored.Any(n => n.UserId == 2 && n.Message == "Resultado cargado"));
        Assert.IsFalse(stored.Any(n => n.UserId == 3));
    }

    [TestMethod]
    public void NotifyJournalistsDelegatesIdGenerationToPersistence()
    {
        userRepository.Add(BuildJournalist(1, "ana@mail.com"));
        userRepository.Add(BuildJournalist(2, "luis@mail.com"));

        service.NotifyJournalists("Resultado cargado");

        IReadOnlyCollection<Notification> stored = notificationRepository.GetAll();

        Assert.AreEqual(2, stored.Count);
        Assert.AreEqual(2, stored.Select(n => n.Id).Distinct().Count());
        Assert.IsTrue(stored.All(n => n.Id > 0));
    }

    [TestMethod]
    public void GetUnreadForUser()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        Notification unread1 = new Notification(1, journalist, "Resultado 1");
        Notification unread2 = new Notification(2, journalist, "Resultado 2");
        Notification read = new Notification(3, journalist, "Resultado 3");
        read.MarkAsRead();

        notificationRepository.Add(unread1);
        notificationRepository.Add(unread2);
        notificationRepository.Add(read);

        IReadOnlyCollection<Notification> unread = service.GetUnreadForUser(JournalistId);

        Assert.AreEqual(2, unread.Count);
        Assert.IsTrue(unread.All(n => !n.IsRead));
        Assert.IsTrue(unread.Any(n => n.Id == 1));
        Assert.IsTrue(unread.Any(n => n.Id == 2));
        Assert.IsFalse(unread.Any(n => n.Id == 3));
    }

    [TestMethod]
    public void MarkAsReadFromService()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        notificationRepository.Add(new Notification(1, journalist, "Resultado cargado"));

        service.MarkAsRead(1, JournalistId);

        IReadOnlyCollection<Notification> unread = service.GetUnreadForUser(JournalistId);

        Assert.AreEqual(0, unread.Count);

        Notification? stored = notificationRepository.GetById(1);
        Assert.IsNotNull(stored);
        Assert.IsTrue(stored!.IsRead);
    }

    [TestMethod]
    public void MarkAsReadForAnotherUserThrows()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        notificationRepository.Add(new Notification(1, journalist, "Resultado cargado"));

        Assert.ThrowsException<ArgumentException>(() => service.MarkAsRead(1, OtherUserId));

        Notification? stored = notificationRepository.GetById(1);
        Assert.IsNotNull(stored);
        Assert.IsFalse(stored!.IsRead);
    }

    [TestMethod]
    public void MarkAsReadLogsAuditEntry()
    {
        MockAuditLogRepository auditRepository = new MockAuditLogRepository();
        AuditLogService auditLogService = AuditLogService.CreateInstance(auditRepository);
        NotificationService auditedService = new NotificationService(
            notificationRepository,
            userRepository,
            auditLogService);

        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        notificationRepository.Add(new Notification(1, journalist, "Resultado cargado"));

        auditedService.MarkAsRead(1, JournalistId);

        List<AuditLog> logs = auditRepository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual(AuditActions.NotificationRead, logs[0].Action);
    }

    [TestMethod]
    public void NotifyResultLoadedNotifiesJournalistsWhenResultChanged()
    {
        userRepository.Add(BuildJournalist(JournalistId, "ana@mail.com"));

        Match match = BuildGroupMatch();
        match.SetGoals(2, 1);

        service.NotifyResultLoaded(match, previousHomeGoals: 0, previousAwayGoals: 0);

        IReadOnlyCollection<Notification> stored = notificationRepository.GetAll();
        Assert.AreEqual(1, stored.Count);
        StringAssert.Contains(stored.First().Message, "2-1");
    }

    [TestMethod]
    public void NotifyResultLoadedDoesNotNotifyWhenResultUnchanged()
    {
        userRepository.Add(BuildJournalist(JournalistId, "ana@mail.com"));

        Match match = BuildGroupMatch();
        match.SetGoals(2, 1);

        service.NotifyResultLoaded(match, previousHomeGoals: 2, previousAwayGoals: 1);

        Assert.AreEqual(0, notificationRepository.GetAll().Count);
    }

    [TestMethod]
    public void NotifyMatchSimulatedNotifiesJournalists()
    {
        userRepository.Add(BuildJournalist(JournalistId, "ana@mail.com"));

        Match match = BuildGroupMatch();
        match.SetGoals(3, 0);

        service.NotifyMatchSimulated(match);

        IReadOnlyCollection<Notification> stored = notificationRepository.GetAll();
        Assert.AreEqual(1, stored.Count);
        StringAssert.Contains(stored.First().Message, "3-0");
    }

    [TestMethod]
    public void CountUnreadForUserReturnsOnlyUnreadOfThatUser()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        User other = BuildJournalist(2, "luis@mail.com");
        userRepository.Add(journalist);
        userRepository.Add(other);

        notificationRepository.Add(new Notification(1, journalist, "Resultado 1"));
        notificationRepository.Add(new Notification(2, journalist, "Resultado 2"));
        Notification read = new Notification(3, journalist, "Resultado 3");
        read.MarkAsRead();
        notificationRepository.Add(read);
        notificationRepository.Add(new Notification(4, other, "Resultado 4"));

        int count = service.CountUnreadForUser(JournalistId);

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void CountUnreadForUserReturnsZeroWhenUserHasNoNotifications()
    {
        Assert.AreEqual(0, service.CountUnreadForUser(JournalistId));
    }

    [TestMethod]
    public void MarkAllAsReadMarksEveryUnreadNotificationOfUser()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        notificationRepository.Add(new Notification(1, journalist, "Resultado 1"));
        notificationRepository.Add(new Notification(2, journalist, "Resultado 2"));

        service.MarkAllAsRead(JournalistId);

        Assert.AreEqual(0, service.CountUnreadForUser(JournalistId));
        Assert.IsTrue(notificationRepository.GetAll().All(n => n.IsRead));
    }

    [TestMethod]
    public void MarkAllAsReadDoesNotAffectOtherUsers()
    {
        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        User other = BuildJournalist(2, "luis@mail.com");
        userRepository.Add(journalist);
        userRepository.Add(other);

        notificationRepository.Add(new Notification(1, journalist, "Resultado 1"));
        notificationRepository.Add(new Notification(2, other, "Resultado 2"));

        service.MarkAllAsRead(JournalistId);

        Assert.AreEqual(1, service.CountUnreadForUser(2));
    }

    [TestMethod]
    public void MarkAllAsReadLogsSingleAuditEntry()
    {
        MockAuditLogRepository auditRepository = new MockAuditLogRepository();
        AuditLogService auditLogService = AuditLogService.CreateInstance(auditRepository);
        NotificationService auditedService = new NotificationService(
            notificationRepository,
            userRepository,
            auditLogService);

        User journalist = BuildJournalist(JournalistId, "ana@mail.com");
        userRepository.Add(journalist);

        notificationRepository.Add(new Notification(1, journalist, "Resultado 1"));
        notificationRepository.Add(new Notification(2, journalist, "Resultado 2"));

        auditedService.MarkAllAsRead(JournalistId);

        List<AuditLog> logs = auditRepository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual(AuditActions.NotificationReadAll, logs[0].Action);
    }

    [TestMethod]
    public void MarkAllAsReadWithoutUnreadDoesNotLogAudit()
    {
        MockAuditLogRepository auditRepository = new MockAuditLogRepository();
        AuditLogService auditLogService = AuditLogService.CreateInstance(auditRepository);
        NotificationService auditedService = new NotificationService(
            notificationRepository,
            userRepository,
            auditLogService);

        auditedService.MarkAllAsRead(JournalistId);

        Assert.AreEqual(0, auditRepository.GetAll().Count);
    }

    private static User BuildJournalist(int id, string email)
    {
        User journalist = new User(
            id,
            "Ana",
            "Lopez",
            email,
            "Pass123@",
            new DateTime(1990, 5, 15)
        );
        journalist.AddRole(UserRoles.Periodista);
        return journalist;
    }

    private static MatchGroup BuildGroupMatch()
    {
        TournamentSettings settings = new TournamentSettings(
            id: 1,
            startDate: new DateTime(2026, 6, 1),
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: 0,
            crossingsSeed: 0,
            simulationSeed: 0,
            completionSeed: 0);

        Team home = new Team(
            id: 1,
            name: "Uruguay",
            fifaRanking: 1500,
            isFictitious: false,
            confederation: Confederation.CONMEBOL);

        Team away = new Team(
            id: 2,
            name: "Francia",
            fifaRanking: 1600,
            isFictitious: false,
            confederation: Confederation.UEFA);

        Stadium stadium = new Stadium("Estadio Test", "Ciudad", string.Empty, 20000);

        Group group = new Group("A");
        group.AddTeam(home);
        group.AddTeam(away);

        Journey journey = Journey.CreateInstance(1);

        return MatchGroup.CreateMatchWithConfig(
            100, settings.StartDate, home, away, stadium, settings, group, journey);
    }
}
