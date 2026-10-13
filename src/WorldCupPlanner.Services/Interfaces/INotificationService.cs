using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Interfaces;

public interface INotificationService
{
    void NotifyJournalists(string message);
    void NotifyResultLoaded(Match match, int previousHomeGoals, int previousAwayGoals);
    void NotifyMatchSimulated(Match match);
    IReadOnlyCollection<Notification> GetUnreadForUser(int userId);
    int CountUnreadForUser(int userId);
    void MarkAsRead(int notificationId, int userId);
    void MarkAllAsRead(int userId);
}
