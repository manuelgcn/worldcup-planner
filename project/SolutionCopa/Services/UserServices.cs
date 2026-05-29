using ProjectCopa;
using Services.Interfaces;
using DataAccess.Interfaces;

namespace Services;

public class UserServices
{
    private readonly IAuditLogService? _auditLogService;
    private readonly IUserRepository _userRepository;

    public UserServices(IUserRepository userRepository, IAuditLogService? auditLogService = null)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }

    public IReadOnlyCollection<User> GetAllUsers()
    {
        return _userRepository.GetAll();
    }

    public void AddUser(User user)
    {
        var existingUsers = _userRepository.GetAll();

        if (existingUsers.Any(u => u.email == user.email))
            throw new ArgumentException("Email already exists");

        if (existingUsers.Any(u => u.idUser == user.idUser))
            throw new ArgumentException("User ID is already assigned");

        if (!user.Roles.Any())
            throw new ArgumentException("User must have at least one role");

        _userRepository.Add(user);

        _auditLogService?.Log(
            AuditActions.CreateUser,
            $"UserId={user.idUser} created with email={user.email}.",
            nameof(User),
            user.idUser
        );
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
        User? user = _userRepository.GetByEmail(email);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    public User GetUserById(int id)
    {
        User? user = _userRepository.GetAll().FirstOrDefault(u => u.idUser == id);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    public void RemoveUser(int id)
    {
        if (id == 1)
            throw new ArgumentException("El usuario administrador del sistema no puede ser eliminado");

        User user = GetUserById(id);

        _userRepository.Remove(user);

        _auditLogService?.Log(
            AuditActions.DeleteUser,
            $"UserId={user.idUser} deleted.",
            nameof(User),
            user.idUser
        );
    }

    public void UpdateUser(int id, string name, string lastname, string email)
    {
        if (id == 1)
            throw new ArgumentException("El usuario administrador del sistema no puede ser modificado");

        var users = _userRepository.GetAll();

        if (users.Any(u => u.email == email && u.idUser != id))
            throw new ArgumentException("Email already exists");

        User user = GetUserById(id);

        user.UpdatePersonalData(name, lastname, email);

        _userRepository.Update(user);

        _auditLogService?.Log(
            AuditActions.UpdateUser,
            $"UserId={user.idUser} updated.",
            nameof(User),
            user.idUser
        );
    }

    public void AssignRole(int userId, UserRoles role)
    {
        User user = GetUserById(userId);

        user.AddRole(role);

        _userRepository.Update(user);

        _auditLogService?.Log(
            AuditActions.AssignUserRole,
            $"Role={role} assigned to UserId={user.idUser}.",
            nameof(User),
            user.idUser
        );
    }
}
