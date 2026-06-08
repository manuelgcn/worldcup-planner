using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public NotificationRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(Notification notification)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();

        User? existingUser = context.Users.FirstOrDefault(u => u.idUser == notification.UserId);

        if (existingUser is null)
        {
            context.Users.Add(notification.User);
        }
        else
        {
            notification.User = existingUser;
        }

        context.Notifications.Add(notification);
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
}