using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Repositories;

public class NotificationRepository : INotificationRepository
{
    private const string UserNotFoundMessage = "Cannot store a notification for a user that does not exist.";
    private const string NotificationNotFoundMessage = "Cannot update a notification that does not exist.";

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public NotificationRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Notification notification)
    {
        AddRange(new[] { notification });
    }

    // Un contexto, una consulta de usuarios y un SaveChanges para toda la tanda.
    public void AddRange(IReadOnlyCollection<Notification> notifications)
    {
        if (notifications.Count == 0)
        {
            return;
        }

        using AppDbContext context = _contextFactory.CreateDbContext();

        List<int> userIds = notifications.Select(n => n.UserId).Distinct().ToList();
        Dictionary<int, User> existingUsers = context.Users
            .Where(u => userIds.Contains(u.idUser))
            .ToDictionary(u => u.idUser);

        foreach (Notification notification in notifications)
        {
            notification.User = existingUsers.GetValueOrDefault(notification.UserId)
                ?? throw new InvalidOperationException(UserNotFoundMessage);
        }

        context.Notifications.AddRange(notifications);
        context.SaveChanges();
    }

    public Notification? GetById(int id)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        return context.Notifications
            .Include(n => n.User)
            .FirstOrDefault(n => n.Id == id);
    }

    public IReadOnlyCollection<Notification> GetUnreadByUserId(int userId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        return context.Notifications
            .Include(n => n.User)
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();
    }

    public int CountUnreadByUserId(int userId)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        return context.Notifications
            .Count(n => n.UserId == userId && !n.IsRead);
    }

    public void Update(Notification notification)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        Notification existing = context.Notifications.FirstOrDefault(n => n.Id == notification.Id)
            ?? throw new InvalidOperationException(NotificationNotFoundMessage);

        if (notification.IsRead && !existing.IsRead)
        {
            existing.MarkAsRead();
        }

        context.SaveChanges();
    }

    public IReadOnlyCollection<Notification> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        return context.Notifications
            .Include(n => n.User)
            .ToList();
    }
}
