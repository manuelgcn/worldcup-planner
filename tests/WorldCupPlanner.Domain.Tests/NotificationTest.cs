using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public class NotificationTest
{
    [TestMethod]
    public void CanCreateUnreadNotification()
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

        Assert.AreEqual(1, notification.Id);
        Assert.AreEqual(user, notification.User);
        Assert.AreEqual(user.idUser, notification.UserId);
        Assert.AreEqual("Resultado cargado", notification.Message);
        Assert.IsFalse(notification.IsRead);
    }
    
    
    [TestMethod]
    public void MarkAsReadSetsNotificationAsRead()
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

        notification.MarkAsRead();

        Assert.IsTrue(notification.IsRead);
    }

    
    [TestMethod]
    public void CannotCreateNotificationWithEmptyMessage()
    {
        User user = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        Assert.ThrowsException<ArgumentException>(() =>
            new Notification(
                1,
                user,
                ""
            )
        );
    }

    
    
    
}