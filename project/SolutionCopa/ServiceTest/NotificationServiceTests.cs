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
    
    
    [TestMethod]
    public void GetUnreadForUser()
    {
        MockNotificationRepository notificationRepository = new MockNotificationRepository();
        MockUserRepository userRepository = new MockUserRepository();

        User journalist = new User(
            1,
            "Ana",
            "Lopez",
            "ana@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );
        journalist.AddRole(UserRoles.Periodista);

        userRepository.Add(journalist);

        Notification unread1 = new Notification(1, journalist, "Resultado 1");
        Notification unread2 = new Notification(2, journalist, "Resultado 2");
        Notification read = new Notification(3, journalist, "Resultado 3");
        read.MarkAsRead();

        notificationRepository.Add(unread1);
        notificationRepository.Add(unread2);
        notificationRepository.Add(read);

        NotificationService service = new NotificationService(notificationRepository, userRepository);

        IReadOnlyCollection<Notification> unread = service.GetUnreadForUser(1);

        Assert.AreEqual(2, unread.Count);
        Assert.IsTrue(unread.All(n => !n.IsRead));
        Assert.IsTrue(unread.Any(n => n.Id == 1));
        Assert.IsTrue(unread.Any(n => n.Id == 2));
        Assert.IsFalse(unread.Any(n => n.Id == 3));
    }
    
    
    
    [TestMethod]
    public void MarkAsReadFromService()
    {
        MockNotificationRepository notificationRepository = new MockNotificationRepository();
        MockUserRepository userRepository = new MockUserRepository();

        User journalist = new User(
            1,
            "Ana",
            "Lopez",
            "ana@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );
        journalist.AddRole(UserRoles.Periodista);

        userRepository.Add(journalist);

        Notification notification = new Notification(
            1,
            journalist,
            "Resultado cargado"
        );

        notificationRepository.Add(notification);

        NotificationService service = new NotificationService(notificationRepository, userRepository);

        service.MarkAsRead(1);

        IReadOnlyCollection<Notification> unread = service.GetUnreadForUser(1);

        Assert.AreEqual(0, unread.Count);

        Notification? stored = notificationRepository.GetById(1);
        Assert.IsNotNull(stored);
        Assert.IsTrue(stored!.IsRead);
    }

}