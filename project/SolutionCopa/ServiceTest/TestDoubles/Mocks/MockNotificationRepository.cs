using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public class MockNotificationRepository : INotificationRepository
{
    private readonly List<Notification> notifications = new();

    public void Add(Notification notification)
    {
        notifications.Add(notification);
    }

    public Notification? GetById(int id)
    {
        return notifications.FirstOrDefault(n => n.Id == id);
    }

    public IReadOnlyCollection<Notification> GetUnreadByUserId(int userId)
    {
        return notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();
    }

    public void Update(Notification notification)
    {
        Notification? existing = notifications.FirstOrDefault(n => n.Id == notification.Id);
        if (existing is null)
        {
            notifications.Add(notification);
        }
        else if (notification.IsRead && !existing.IsRead)
        {
            existing.MarkAsRead();
        }
    }

    public IReadOnlyCollection<Notification> GetAll()
    {
        return notifications.ToList();
    }
}