using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class NotificationServiceTests
{
    [TestMethod]
    public void NotifyJournalists()
    {
        MockNotificationRepository notificationRepository = new MockNotificationRepository();
        MockUserRepository userRepository = new MockUserRepository();

        User journalist1 = new User(
            1,
            "Ana",
            "Lopez",
            "ana@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );
        journalist1.AddRole(UserRoles.Periodista);

        User journalist2 = new User(
            2,
            "Luis",
            "Perez",
            "luis@mail.com",
            "Pass123@",
            new DateTime(1991, 3, 10)
        );
        journalist2.AddRole(UserRoles.Periodista);

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

        NotificationService service = new NotificationService(notificationRepository, userRepository);

        service.NotifyJournalists("Resultado cargado");

        IReadOnlyCollection<Notification> stored = notificationRepository.GetAll();

        Assert.AreEqual(2, stored.Count);
        Assert.IsTrue(stored.All(n => !n.IsRead));
        Assert.IsTrue(stored.Any(n => n.UserId == 1 && n.Message == "Resultado cargado"));
        Assert.IsTrue(stored.Any(n => n.UserId == 2 && n.Message == "Resultado cargado"));
        Assert.IsFalse(stored.Any(n => n.UserId == 3));
    }
}