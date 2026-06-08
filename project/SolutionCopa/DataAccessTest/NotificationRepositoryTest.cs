using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class NotificationRepositoryTest
{
    private NotificationRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new NotificationRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresNotificationAndGetByIdReturnsIt()
    {
        User user = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

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
    public void GetUnreadByUserIdTest()
    {
        User firstUser = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        User secondUser = new User(
            2,
            "Ana",
            "Lopez",
            "ana@mail.com",
            "Pass123@",
            new DateTime(1992, 7, 10)
        );

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
}