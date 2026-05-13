
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
        _auditLogService = auditLogService;
    }

    public void AddUser(User user)
    {
        if (users.Any(u => u.email == user.email))
            throw new ArgumentException("Email already exists");
        if (users.Any(u => u.idUser == user.idUser))
            throw new ArgumentException("User ID is already assigned");
        if (!user.Roles.Any())
            throw new ArgumentException("User must have at least one role");


        users.Add(user);
        _auditLogService?.Log(
            AuditActions.CreateUser,
            $"UserId={user.idUser} created with email={user.email}.",
            nameof(User),
            user.idUser);
    }
    
    
    
    public void RemoveRole(int userId, UserRoles role)
    {
        User user = GetUserById(userId);

        if (!user.Roles.Contains(role))
            return;

        if (user.Roles.Count == 1)
            throw new ArgumentException("User must have at least one role");

        user.RemoveRoles(role);

        _auditLogService?.Log(
            AuditActions.RemoveUserRoles,
            $"Role={role} removed from UserId={user.idUser}.",
            nameof(User),
            user.idUser
        );
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

        if (id == 1)
        {
            throw new ArgumentException("El usuario administrador del sistema no puede ser modificado");
        }

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
