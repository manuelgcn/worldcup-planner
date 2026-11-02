using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class NotificationRepositoryTest
{
    private InMemoryDbContextFactory _contextFactory = null!;
    private NotificationRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryDbContextFactory();
        _repository = new NotificationRepository(_contextFactory);
    }

    [TestMethod]
    public void AddStoresNotificationAndGetByIdReturnsIt()
    {
        User user = SeedUser(1, "juan@mail.com");

        Notification notification = new Notification(
            1,
            user,
            "Resultado cargado"
        );

        _repository.Add(notification);

        Notification? stored = _repository.GetById(1);

        Assert.IsNotNull(stored);
        Assert.AreEqual(1, stored!.Id);
        Assert.AreEqual(1, stored.UserId);
        Assert.AreEqual("Resultado cargado", stored.Message);
        Assert.IsFalse(stored.IsRead);
    }

    [TestMethod]
    public void AddRangeStoresTheWholeBatchForSeveralUsers()
    {
        User ana = SeedUser(1, "ana@mail.com");
        User luis = SeedUser(2, "luis@mail.com");

        _repository.AddRange(new List<Notification>
        {
            new Notification(0, ana, "Partido 1"),
            new Notification(0, luis, "Partido 1"),
            new Notification(0, ana, "Partido 2"),
        });

        IReadOnlyCollection<Notification> stored = _repository.GetAll();
        Assert.AreEqual(3, stored.Count);
        Assert.AreEqual(2, stored.Count(n => n.UserId == 1));
        Assert.AreEqual(3, stored.Select(n => n.Id).Distinct().Count());
    }

    [TestMethod]
    public void AddRangeWithAnUnknownUserStoresNothing()
    {
        User ana = SeedUser(1, "ana@mail.com");
        User ghost = new User(99, "Ghost", "User", "ghost@mail.com", "Pass123@", new DateTime(1990, 1, 1));

        Assert.ThrowsException<InvalidOperationException>(() => _repository.AddRange(new List<Notification>
        {
            new Notification(0, ana, "Partido 1"),
            new Notification(0, ghost, "Partido 1"),
        }));

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void AddGeneratesIdWhenIdIsZero()
    {
        User user = SeedUser(1, "juan@mail.com");

        Notification first = new Notification(0, user, "Resultado 1");
        Notification second = new Notification(0, user, "Resultado 2");

        _repository.Add(first);
        _repository.Add(second);

        IReadOnlyCollection<Notification> stored = _repository.GetAll();

        Assert.AreEqual(2, stored.Count);
        Assert.IsTrue(stored.All(n => n.Id > 0));
        Assert.AreEqual(2, stored.Select(n => n.Id).Distinct().Count());
    }

    [TestMethod]
    public void AddThrowsWhenUserDoesNotExist()
    {
        User unsavedUser = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        Notification notification = new Notification(1, unsavedUser, "Resultado cargado");

        Assert.ThrowsException<InvalidOperationException>(() => _repository.Add(notification));
    }

    [TestMethod]
    public void GetUnreadByUserIdTest()
    {
        User firstUser = SeedUser(1, "juan@mail.com");
        User secondUser = SeedUser(2, "ana@mail.com");

        Notification unreadForFirstUser = new Notification(
            1,
            firstUser,
            "Resultado 1"
        );

        Notification readForFirstUser = new Notification(
            2,
            firstUser,
            "Resultado 2"
        );
        readForFirstUser.MarkAsRead();

        Notification unreadForSecondUser = new Notification(
            3,
            secondUser,
            "Resultado 3"
        );

        _repository.Add(unreadForFirstUser);
        _repository.Add(readForFirstUser);
        _repository.Add(unreadForSecondUser);

        IReadOnlyCollection<Notification> unread = _repository.GetUnreadByUserId(1);

        Assert.AreEqual(1, unread.Count);
        Assert.AreEqual(1, unread.First().Id);
        Assert.AreEqual("Resultado 1", unread.First().Message);
    }

    [TestMethod]
    public void UpdatePersistsNotificationAsRead()
    {
        User user = SeedUser(1, "juan@mail.com");

        Notification notification = new Notification(
            1,
            user,
            "Resultado cargado"
        );

        _repository.Add(notification);

        notification.MarkAsRead();
        _repository.Update(notification);

        IReadOnlyCollection<Notification> unread = _repository.GetUnreadByUserId(1);

        Assert.AreEqual(0, unread.Count);

        Notification? stored = _repository.GetById(1);
        Assert.IsNotNull(stored);
        Assert.IsTrue(stored!.IsRead);
    }

    [TestMethod]
    public void UpdateThrowsWhenNotificationDoesNotExist()
    {
        User user = SeedUser(1, "juan@mail.com");

        Notification notification = new Notification(1, user, "Resultado cargado");

        Assert.ThrowsException<InvalidOperationException>(() => _repository.Update(notification));
    }

    [TestMethod]
    public void CountUnreadByUserIdCountsOnlyUnreadOfThatUser()
    {
        User firstUser = SeedUser(1, "juan@mail.com");
        User secondUser = SeedUser(2, "ana@mail.com");

        Notification unread1 = new Notification(1, firstUser, "Resultado 1");
        Notification unread2 = new Notification(2, firstUser, "Resultado 2");
        Notification read = new Notification(3, firstUser, "Resultado 3");
        read.MarkAsRead();
        Notification otherUser = new Notification(4, secondUser, "Resultado 4");

        _repository.Add(unread1);
        _repository.Add(unread2);
        _repository.Add(read);
        _repository.Add(otherUser);

        Assert.AreEqual(2, _repository.CountUnreadByUserId(1));
    }

    [TestMethod]
    public void CountUnreadByUserIdReturnsZeroWhenUserHasNoNotifications()
    {
        SeedUser(1, "juan@mail.com");

        Assert.AreEqual(0, _repository.CountUnreadByUserId(1));
    }

    [TestMethod]
    public void MarkAllAsRead_MarksOnlyThatUsersUnread()
    {
        User firstUser = SeedUser(1, "juan@mail.com");
        User secondUser = SeedUser(2, "ana@mail.com");

        Notification alreadyRead = new Notification(3, firstUser, "Resultado 3");
        alreadyRead.MarkAsRead();
        _repository.Add(new Notification(1, firstUser, "Resultado 1"));
        _repository.Add(new Notification(2, firstUser, "Resultado 2"));
        _repository.Add(alreadyRead);
        _repository.Add(new Notification(4, secondUser, "Resultado 4"));

        int marked = _repository.MarkAllAsRead(1);

        Assert.AreEqual(2, marked);
        Assert.AreEqual(0, _repository.CountUnreadByUserId(1));
        Assert.AreEqual(1, _repository.CountUnreadByUserId(2));
    }

    [TestMethod]
    public void GetUnreadByUserId_ReturnsNewestFirst()
    {
        User user = SeedUser(1, "juan@mail.com");
        DateTime noon = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        AddNotificationCreatedAt(1, user, noon.AddHours(-2));
        AddNotificationCreatedAt(2, user, noon);
        AddNotificationCreatedAt(3, user, noon.AddHours(-1));

        List<int> ids = _repository.GetUnreadByUserId(1).Select(n => n.Id).ToList();

        CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, ids);
    }

    // CreatedAt lo fija el constructor con la hora actual: se pisa en la base para controlar el orden.
    private void AddNotificationCreatedAt(int id, User user, DateTime createdAt)
    {
        _repository.Add(new Notification(id, user, $"Resultado {id}"));

        using AppDbContext context = _contextFactory.CreateDbContext();
        Notification stored = context.Notifications.Single(n => n.Id == id);
        context.Entry(stored).Property(n => n.CreatedAt).CurrentValue = createdAt;
        context.SaveChanges();
    }

    private User SeedUser(int id, string email)
    {
        User user = new User(
            id,
            "Juan",
            "Perez",
            email,
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        using AppDbContext context = _contextFactory.CreateDbContext();
        context.Users.Add(user);
        context.SaveChanges();

        return user;
    }
}
