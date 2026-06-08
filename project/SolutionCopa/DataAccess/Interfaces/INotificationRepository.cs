using ProjectCopa;

namespace DataAccess.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
    Notification? GetById(int id);
}