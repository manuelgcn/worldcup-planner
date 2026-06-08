using ProjectCopa;

namespace Services.Interfaces;

public interface INotificationService
{
    void NotifyJournalists(string message);
    IReadOnlyCollection<Notification> GetUnreadForUser(int userId);
    void MarkAsRead(int notificationId);
}