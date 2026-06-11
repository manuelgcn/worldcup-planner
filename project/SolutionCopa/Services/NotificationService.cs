using DataAccess.Interfaces;
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class NotificationService : INotificationService
{
    private const string NotificationNotFoundMessage = "Notification was not found.";
    private const string NotificationOwnershipMessage = "Notification does not belong to the user.";
    private const string ResultLoadedMessageTemplate = "Se cargo resultado del partido {0}: {1} {2}-{3} {4}.";
    private const string MatchSimulatedMessageTemplate = "Se simulo resultado del partido {0}: {1} {2}-{3} {4}.";
    private const string DefaultHomeTeamName = "Equipo local";
    private const string DefaultAwayTeamName = "Equipo visitante";
    private const int IdGeneratedByPersistence = 0;

    private readonly INotificationRepository notificationRepository;
    private readonly IUserRepository userRepository;
    private readonly IAuditLogService? auditLogService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IAuditLogService? auditLogService = null)
    {
        this.notificationRepository = notificationRepository;
        this.userRepository = userRepository;
        this.auditLogService = auditLogService;
    }

    public void NotifyJournalists(string message)
    {
        IEnumerable<User> journalists = userRepository.GetAll()
            .Where(u => u.HasRole(UserRoles.Periodista));

        foreach (User journalist in journalists)
        {
            notificationRepository.Add(new Notification(IdGeneratedByPersistence, journalist, message));
        }
    }

    public void NotifyResultLoaded(Match match, int previousHomeGoals, int previousAwayGoals)
    {
        ArgumentNullException.ThrowIfNull(match);

        if (match.HomeGoals == previousHomeGoals && match.AwayGoals == previousAwayGoals)
        {
            return;
        }

        NotifyJournalists(BuildMatchMessage(ResultLoadedMessageTemplate, match));
    }

    public void NotifyMatchSimulated(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        NotifyJournalists(BuildMatchMessage(MatchSimulatedMessageTemplate, match));
    }

    public IReadOnlyCollection<Notification> GetUnreadForUser(int userId)
    {
        return notificationRepository.GetUnreadByUserId(userId);
    }

    public void MarkAsRead(int notificationId, int userId)
    {
        Notification? notification = notificationRepository.GetById(notificationId);

        if (notification is null)
        {
            throw new ArgumentException(NotificationNotFoundMessage);
        }

        if (notification.UserId != userId)
        {
            throw new ArgumentException(NotificationOwnershipMessage);
        }

        notification.MarkAsRead();
        notificationRepository.Update(notification);

        auditLogService?.Log(
            AuditActions.NotificationRead,
            $"Notification {notificationId} marked as read by user {userId}.",
            affectedEntityType: nameof(Notification),
            affectedEntityId: notificationId);
    }

    private static string BuildMatchMessage(string template, Match match)
    {
        string home = match.HomeTeam?.Name ?? DefaultHomeTeamName;
        string away = match.AwayTeam?.Name ?? DefaultAwayTeamName;
        return string.Format(template, match.MatchId, home, match.HomeGoals, match.AwayGoals, away);
    }
}
