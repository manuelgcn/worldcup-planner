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
}