using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
    void AddRange(IReadOnlyCollection<Notification> notifications);
    Notification? GetById(int id);
    IReadOnlyCollection<Notification> GetUnreadByUserId(int userId);
    int CountUnreadByUserId(int userId);
    void Update(Notification notification);
    // Marca leidas todas las no leidas del usuario en una sola escritura; devuelve cuantas marco.
    int MarkAllAsRead(int userId);
    IReadOnlyCollection<Notification> GetAll();
}