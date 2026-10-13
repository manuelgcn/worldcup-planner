using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class NotificationService : INotificationService
{
    private const string NotificationNotFoundMessage = "No se encontro la notificacion.";
    private const string NotificationOwnershipMessage = "La notificacion no pertenece al usuario.";
    private const string ResultLoadedMessageTemplate = "Se cargo resultado del partido {0}: {1} {2}-{3} {4}.";
    private const string MatchSimulatedMessageTemplate = "Se simulo resultado del partido {0}: {1} {2}-{3} {4}.";
    private const string DefaultHomeTeamName = "Equipo local";
    private const string DefaultAwayTeamName = "Equipo visitante";
    private const int IdGeneratedByPersistence = 0;

    private readonly INotificationRepository notificationRepository;
    private readonly IUserRepository userRepository;
    private readonly IAuditLogService auditLogService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(notificationRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);
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

    public void NotifyMatchesSimulated(IReadOnlyCollection<Match> matches)
    {
        foreach (Match match in matches)
        {
            NotifyMatchSimulated(match);
        }
    }

    public IReadOnlyCollection<Notification> GetUnreadForUser(int userId)
    {
        return notificationRepository.GetUnreadByUserId(userId);
    }

    public int CountUnreadForUser(int userId)
    {
        return notificationRepository.CountUnreadByUserId(userId);
    }

    public void MarkAllAsRead(int userId)
    {
        IReadOnlyCollection<Notification> unread = notificationRepository.GetUnreadByUserId(userId);

        if (unread.Count == 0)
        {
            return;
        }

        foreach (Notification notification in unread)
        {
            notification.MarkAsRead();
            notificationRepository.Update(notification);
        }

        auditLogService.Log(
            AuditActions.NotificationReadAll,
            $"All notifications ({unread.Count}) marked as read by user {userId}.",
            affectedEntityType: nameof(Notification),
            affectedEntityId: userId);
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

        auditLogService.Log(
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
