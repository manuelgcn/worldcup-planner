using ProjectCopa;

namespace DataAccess.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
    Notification? GetById(int id);
    IReadOnlyCollection<Notification> GetUnreadByUserId(int userId);
    int CountUnreadByUserId(int userId);
    void Update(Notification notification);
    IReadOnlyCollection<Notification> GetAll();
}