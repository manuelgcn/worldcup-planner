namespace WorldCupPlanner.Domain;

public class Notification
{
    private const string EmptyMessageError = "El mensaje de la notificacion es obligatorio.";

    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRead { get; private set; }

    public Notification(int id, User user, string message)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateMessage(message);

        Id = id;
        User = user;
        UserId = user.idUser;
        Message = message;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public Notification()
    {
        User = null!;
        Message = "";
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(EmptyMessageError, nameof(message));
        }
    }
    public void MarkAsRead()
    {
        IsRead = true;
    }
}