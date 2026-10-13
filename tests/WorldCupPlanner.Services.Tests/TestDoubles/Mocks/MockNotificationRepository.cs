using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public class MockNotificationRepository : INotificationRepository
{
    private const string NotificationNotFoundMessage = "Cannot update a notification that does not exist.";

    private readonly List<Notification> notifications = new();
    private int nextId = 1;

    public void Add(Notification notification)
    {
        if (notification.Id == 0)
        {
            notification.Id = nextId;
        }

        nextId = Math.Max(nextId, notification.Id) + 1;
        notifications.Add(notification);
    }

    public int AddRangeCalls { get; private set; }

    public void AddRange(IReadOnlyCollection<Notification> notifications)
    {
        AddRangeCalls++;
        foreach (Notification notification in notifications)
        {
            Add(notification);
        }
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

    public int CountUnreadByUserId(int userId)
    {
        return notifications.Count(n => n.UserId == userId && !n.IsRead);
    }

    public void Update(Notification notification)
    {
        Notification existing = notifications.FirstOrDefault(n => n.Id == notification.Id)
            ?? throw new InvalidOperationException(NotificationNotFoundMessage);

        if (notification.IsRead && !existing.IsRead)
        {
            existing.MarkAsRead();
        }
    }

    public IReadOnlyCollection<Notification> GetAll()
    {
        return notifications.ToList();
    }
}
