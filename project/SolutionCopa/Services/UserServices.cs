
using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class UserServices
{
    private readonly IAuditLogService? _auditLogService;
    private readonly List<User> users = new List<User>();

    public IReadOnlyCollection<User> GetAllUsers()
    {
        return users.AsReadOnly();
    }

    public UserServices(IAuditLogService? auditLogService = null)
    {
        this._auditLogService = auditLogService;
    }

    public void AddUser(User user)
    {
        if (users.Any(u => u.email == user.email))
            throw new ArgumentException("Email already exists");
        if (users.Any(u => u.idUser == user.idUser))
            throw new ArgumentException("User ID is already assigned");

        users.Add(user);
        _auditLogService?.Log(
            AuditActions.CreateUser,
            $"UserId={user.idUser} created with email={user.email}.",
            nameof(User),
            user.idUser);
    }

    public User GetUserByEmail(string email)
    {
        User user = users.FirstOrDefault(u => u.email == email);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    
    public User GetUserById(int id)
    {
        User user = users.FirstOrDefault(u => u.idUser == id);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    
    public void RemoveUser(int id)
    {
        User user = GetUserById(id);
        users.Remove(user);
        _auditLogService?.Log(
            AuditActions.DeleteUser,
            $"UserId={user.idUser} deleted.",
            nameof(User),
            user.idUser);
    }

    public void UpdateUser(int id, string name, string lastname, string email)
    {
        User user = GetUserById(id);

        if (users.Any(u => u.email == email && u.idUser != id))
            throw new ArgumentException("Email already exists");

        user.UpdatePersonalData(name, lastname, email);
        _auditLogService?.Log(
            AuditActions.UpdateUser,
            $"UserId={user.idUser} updated.",
            nameof(User),
            user.idUser);
    }

    public void AssignRole(int userId, UserRoles role)
    {
        User user = GetUserById(userId);
        user.AddRole(role);
        _auditLogService?.Log(
            AuditActions.AssignUserRole,
            $"Role={role} assigned to UserId={user.idUser}.",
            nameof(User),
            user.idUser);
    }
}
