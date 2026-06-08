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
}