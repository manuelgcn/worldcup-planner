using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;
using DataAccess.Interfaces;

namespace Services;

public class UserServices : IUserService
{
    // Id reservado para el administrador del sistema: no puede modificarse ni eliminarse.
    private const int SystemAdminUserId = 1;

    private const string InvalidCredentialsMessage = "Credenciales invalidas";
    private const string EmailAlreadyExistsMessage = "El email ya esta registrado";
    private const string UserIdAlreadyAssignedMessage = "El id de usuario ya esta asignado";
    private const string UserMustHaveOneRoleMessage = "El usuario debe tener al menos un rol";
    private const string UserNotFoundMessage = "Usuario no encontrado";
    private const string SystemAdminCannotBeModifiedMessage = "El usuario administrador del sistema no puede ser modificado";
    private const string SystemAdminCannotBeDeletedMessage = "El usuario administrador del sistema no puede ser eliminado";

    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;

    public UserServices(IUserRepository userRepository, IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }

    public IReadOnlyCollection<User> GetAllUsers()
    {
        return _userRepository.GetAll();
    }

    public void RegisterUser(CreateUserRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // La construccion de la entidad de dominio (con sus invariantes) vive aca,
        // no en la UI: la pantalla solo arma el request.
        User user = new User(
            request.Id,
            request.Name,
            request.Lastname,
            request.Email,
            request.Password,
            request.DateOfBirth);
        user.AddRole(request.InitialRole);

        AddUser(user);
    }

    public void AddUser(User user)
    {
        if (_userRepository.ExistsByEmail(user.email))
            throw new UserServicesException(EmailAlreadyExistsMessage);

        if (_userRepository.GetById(user.idUser) is not null)
            throw new UserServicesException(UserIdAlreadyAssignedMessage);

        if (!user.Roles.Any())
            throw new UserServicesException(UserMustHaveOneRoleMessage);

        _userRepository.Add(user);

        _auditLogService.Log(
            AuditActions.CreateUser,
            $"UserId={user.idUser} created with email={user.email}.",
            nameof(User),
            user.idUser
        );
    }

    public void RemoveRole(int userId, UserRoles role)
    {
        if (userId == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeModifiedMessage);

        User user = GetUserById(userId);

        if (!user.Roles.Any(r => r.Role == role))
            return;

        if (user.Roles.Count == 1)
            throw new UserServicesException(UserMustHaveOneRoleMessage);

        user.RemoveRoles(role);

        _userRepository.Update(user);

        _auditLogService.Log(
            AuditActions.RemoveUserRoles,
            $"Role={role} removed from UserId={user.idUser}.",
            nameof(User),
            user.idUser
        );
    }


    public User Authenticate(string email, string password)
    {
        User? user = _userRepository.GetByEmail(email);

        if (user == null || !user.PasswordMatches(password))
            throw new UserServicesException(InvalidCredentialsMessage);

        return user;
    }

    public User GetUserByEmail(string email)
    {
        User? user = _userRepository.GetByEmail(email);

        if (user == null)
            throw new UserServicesException(UserNotFoundMessage);

        return user;
    }

    public User GetUserById(int id)
    {
        User? user = _userRepository.GetById(id);

        if (user == null)
            throw new UserServicesException(UserNotFoundMessage);

        return user;
    }

    public void RemoveUser(int id)
    {
        if (id == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeDeletedMessage);

        User user = GetUserById(id);

        _userRepository.Remove(user);

        _auditLogService.Log(
            AuditActions.DeleteUser,
            $"UserId={user.idUser} deleted.",
            nameof(User),
            user.idUser
        );
    }

    public void UpdateUser(int id, string name, string lastname, string email)
    {
        if (id == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeModifiedMessage);

        User? userWithEmail = _userRepository.GetByEmail(email);

        if (userWithEmail is not null && userWithEmail.idUser != id)
            throw new UserServicesException(EmailAlreadyExistsMessage);

        User user = GetUserById(id);

        user.UpdatePersonalData(name, lastname, email);

        _userRepository.Update(user);

        _auditLogService.Log(
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

        _auditLogService.Log(
            AuditActions.AssignUserRole,
            $"Role={role} assigned to UserId={user.idUser}.",
            nameof(User),
            user.idUser
        );
    }
    
    public void ToggleRole(int userId, UserRoles role)
    {
        if (userId == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeModifiedMessage);

        User user = GetUserById(userId);

        bool hasRole = user.Roles.Any(r => r.Role == role);

        if (hasRole)
        {
            if (user.Roles.Count == 1)
                throw new UserServicesException(UserMustHaveOneRoleMessage);

            user.RemoveRoles(role);

            _auditLogService.Log(
                AuditActions.RemoveUserRoles,
                $"Role={role} removed from UserId={user.idUser}.",
                nameof(User),
                user.idUser
            );
        }
        else
        {
            user.AddRole(role);

            _auditLogService.Log(
                AuditActions.AssignUserRole,
                $"Role={role} assigned to UserId={user.idUser}.",
                nameof(User),
                user.idUser
            );
        }

        _userRepository.Update(user);
    }
}
