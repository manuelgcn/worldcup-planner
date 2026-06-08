using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly IUserRepository userRepository;

    public NotificationService(INotificationRepository notificationRepository, IUserRepository userRepository)
    {
        this.notificationRepository = notificationRepository;
        this.userRepository = userRepository;
    }

    public void NotifyJournalists(string message)
    {
        IReadOnlyCollection<User> users = userRepository.GetAll();
        List<User> journalists = users
            .Where(u => u.HasRole(UserRoles.Periodista))
            .ToList();

        int nextId = notificationRepository.GetAll()
            .Select(n => n.Id)
            .DefaultIfEmpty(0)
            .Max() + 1;

        foreach (User journalist in journalists)
        {
            Notification notification = new Notification(nextId, journalist, message);
            notificationRepository.Add(notification);
            nextId++;
        }
    }

    public IReadOnlyCollection<Notification> GetUnreadForUser(int userId)
    {
        return notificationRepository.GetUnreadByUserId(userId);
    }

    public void MarkAsRead(int notificationId)
    {
        Notification? notification = notificationRepository.GetById(notificationId);

        if (notification is null)
        {
            throw new ArgumentException("Notification was not found.");
        }

        notification.MarkAsRead();
        notificationRepository.Update(notification);
    }
}