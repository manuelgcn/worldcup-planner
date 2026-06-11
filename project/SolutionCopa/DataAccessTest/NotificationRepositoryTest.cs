using DataAccess;
using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

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
