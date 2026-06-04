namespace ProjectCopa;

public class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public UserRoles Role { get; set; }
}
