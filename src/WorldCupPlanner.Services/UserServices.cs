using System.Collections.Concurrent;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.DataAccess.Interfaces;

namespace WorldCupPlanner.Services;

public class UserServices : IUserService
{
    // Id reservado para el administrador del sistema: no puede modificarse ni eliminarse.
    private const int SystemAdminUserId = 1;

    // El registro publico es anonimo: nunca debe poder auto-asignarse un rol
    // privilegiado. El servidor ignora el rol del request y fuerza Periodista.
    // Editor/Admin solo se conceden por el camino administrativo (AssignRole).
    private const UserRoles PublicRegistrationRole = UserRoles.Periodista;

    private const string InvalidCredentialsMessage = "Credenciales invalidas";
    private const string TooManyFailedLoginsMessage = "Demasiados intentos fallidos. Probá de nuevo en unos minutos.";
    private const int MaxFailedLogins = 5;
    private const int LockoutMinutes = 5;
    private const int MaxTrackedLogins = 10_000;
    private const int MaxLoginKeyLength = 254;
    private const string EmailAlreadyExistsMessage = "El email ya esta registrado";
    private const string UserIdAlreadyAssignedMessage = "El id de usuario ya esta asignado";
    private const string UserMustHaveOneRoleMessage = "El usuario debe tener al menos un rol";
    private const string UserNotFoundMessage = "Usuario no encontrado";
    private const string SystemAdminCannotBeModifiedMessage = "El usuario administrador del sistema no puede ser modificado";
    private const string SystemAdminCannotBeDeletedMessage = "El usuario administrador del sistema no puede ser eliminado";
    private const string UserCannotDeleteThemselvesMessage = "Un usuario no puede eliminar su propia cuenta";

    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;
    private readonly TimeProvider _timeProvider;

    // Intentos fallidos por clave de login (ver LoginKeyFor). El servicio es singleton: el conteo vive mientras corre la app.
    private readonly ConcurrentDictionary<string, FailedLogins> _failedLogins = new();

    // Cuantas claves de login se estan recordando: deja comprobar que esa memoria esta acotada.
    public int TrackedLoginCount => _failedLogins.Count;

    public UserServices(IUserRepository userRepository, IAuditLogService auditLogService, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyCollection<User> GetAllUsers()
    {
        return _userRepository.GetAll();
    }

    public void RegisterUser(CreateUserRequestDto request)
    {
        // No se confia en request.InitialRole: el registro publico siempre es Periodista.
        AddUser(BuildUser(request, PublicRegistrationRole));
    }

    // Alta administrativa: el rol elegido por el admin se respeta y queda auditado su autor.
    public void CreateUser(CreateUserRequestDto request, string? performedByEmail)
    {
        AddUser(BuildUser(request, request.InitialRole), performedByEmail);
    }

    // La construccion de la entidad de dominio (con sus invariantes) vive aca,
    // no en la UI: la pantalla solo arma el request.
    private static User BuildUser(CreateUserRequestDto request, UserRoles role)
    {
        ArgumentNullException.ThrowIfNull(request);

        User user = new User(
            request.Id,
            request.Name,
            request.Lastname,
            request.Email,
            request.Password,
            request.DateOfBirth);
        user.AddRole(role);
        return user;
    }

    public void AddUser(User user, string? performedByEmail = null)
    {
        if (_userRepository.ExistsByEmail(user.email))
            throw new UserServicesException(EmailAlreadyExistsMessage);

        if (_userRepository.GetById(user.idUser) is not null)
            throw new UserServicesException(UserIdAlreadyAssignedMessage);

        if (!user.Roles.Any())
            throw new UserServicesException(UserMustHaveOneRoleMessage);

        _userRepository.Add(user);

        _auditLogService.LogPerformedBy(
            AuditActions.CreateUser,
            $"UserId={user.idUser} created with email={user.email}.",
            performedByEmail,
            nameof(User),
            user.idUser
        );
    }

    public void RemoveRole(int userId, UserRoles role, string? performedByEmail = null)
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

        _auditLogService.LogPerformedBy(
            AuditActions.RemoveUserRoles,
            $"Role={role} removed from UserId={user.idUser}.",
            performedByEmail,
            nameof(User),
            user.idUser
        );
    }


    public User Authenticate(string email, string password)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        User? user = _userRepository.GetByEmail(email);

        // Si el email es de una cuenta se cuenta bajo su email guardado: la base puede resolver a la misma
        // cuenta un email escrito distinto, y con la clave tipeada cada forma tendria su propio contador.
        // Por eso la lectura va antes del bloqueo; la verificacion de la contrasena, que es lo caro, sigue despues.
        string loginKey = LoginKeyFor(user?.email ?? email);

        if (_failedLogins.TryGetValue(loginKey, out FailedLogins? failed) && failed.IsLockedAt(now))
            throw new UserServicesException(TooManyFailedLoginsMessage);

        // Con un email desconocido se verifica igual una contrasena: si no, la respuesta rapida delataria cuales estan registrados.
        bool passwordMatches = user?.PasswordMatches(password) ?? User.PasswordMatchesNoAccount(password);

        if (user == null || !passwordMatches)
        {
            RegisterFailedLogin(loginKey, now);
            throw new UserServicesException(InvalidCredentialsMessage);
        }

        _failedLogins.TryRemove(loginKey, out _);

        if (user.HasLegacyPasswordHash)
            UpgradeLegacyPasswordHash(user, password);

        return user;
    }

    // Se recorta al largo maximo de un email de cuenta: lo que sobra no puede ser de ningun usuario
    // y, sin recortar, cada clave ocuparia tanto como el texto que mande el cliente.
    private static string LoginKeyFor(string email)
    {
        string key = email.Trim().ToLowerInvariant();
        return key.Length > MaxLoginKeyLength ? key[..MaxLoginKeyLength] : key;
    }

    // Se cuenta también para emails inexistentes: si no, el bloqueo delataría cuáles están registrados.
    private void RegisterFailedLogin(string loginKey, DateTimeOffset now)
    {
        if (_failedLogins.Count >= MaxTrackedLogins && !_failedLogins.ContainsKey(loginKey))
            FreeTrackedLogin(now);

        DateTimeOffset expiresAt = now.AddMinutes(LockoutMinutes);

        _failedLogins.AddOrUpdate(
            loginKey,
            _ => new FailedLogins(1, expiresAt),
            (_, current) =>
                current.ExpiresAt <= now ? new FailedLogins(1, expiresAt)
                : current.Count + 1 >= MaxFailedLogins ? new FailedLogins(MaxFailedLogins, expiresAt)
                : current with { Count = current.Count + 1 });
    }

    // Sin tope, probar emails al azar haria crecer el diccionario sin limite. TryRemove compara tambien
    // el valor, asi no se pierde una entrada que otro hilo acaba de renovar; entre hilos el tope es aproximado.
    // shortcut: con el tope lleno de entradas vigentes se descarta la mas proxima a vencer, asi que
    // MaxTrackedLogins emails nuevos pueden borrar un contador o un bloqueo real, y cada clave nueva
    // recorre todo el diccionario; si eso llega a importar, limitar los intentos por IP antes de este servicio.
    private void FreeTrackedLogin(DateTimeOffset now)
    {
        foreach (KeyValuePair<string, FailedLogins> entry in _failedLogins)
        {
            if (entry.Value.ExpiresAt <= now)
                _failedLogins.TryRemove(entry);
        }

        if (_failedLogins.Count >= MaxTrackedLogins)
            _failedLogins.TryRemove(_failedLogins.MinBy(entry => entry.Value.ExpiresAt));
    }

    // Vence a los LockoutMinutes del primer fallo mientras cuenta, y a los LockoutMinutes del ultimo una vez bloqueado.
    private sealed record FailedLogins(int Count, DateTimeOffset ExpiresAt)
    {
        public bool IsLockedAt(DateTimeOffset now) => Count >= MaxFailedLogins && now < ExpiresAt;
    }

    // Unica oportunidad de reemplazar un hash mas debil que el actual: solo en el login se conoce la contrasena.
    private void UpgradeLegacyPasswordHash(User user, string password)
    {
        user.UpgradeLegacyPasswordHash(password);
        _userRepository.Update(user);

        _auditLogService.LogPerformedBy(
            AuditActions.UpgradePasswordHash,
            $"Password hash of UserId={user.idUser} upgraded to the current format.",
            user.email,
            nameof(User),
            user.idUser
        );
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

    public void RemoveUser(int id, string? performedByEmail = null)
    {
        if (id == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeDeletedMessage);

        EnsureNotSelfDeletion(id, performedByEmail);

        User user = GetUserById(id);

        _userRepository.Remove(user);

        _auditLogService.LogPerformedBy(
            AuditActions.DeleteUser,
            $"UserId={user.idUser} deleted.",
            performedByEmail,
            nameof(User),
            user.idUser
        );
    }

    // Impide la auto-eliminacion: si quien ejecuta la accion (resuelto por su email) es el mismo
    // usuario objetivo, se rechaza. Cuando no se conoce el actor (email nulo o inexistente) no se
    // puede afirmar que sea auto-eliminacion, asi que no se bloquea por esta regla.
    private void EnsureNotSelfDeletion(int targetUserId, string? performedByEmail)
    {
        if (string.IsNullOrWhiteSpace(performedByEmail))
            return;

        User? actor = _userRepository.GetByEmail(performedByEmail);

        if (actor is not null && actor.idUser == targetUserId)
            throw new UserServicesException(UserCannotDeleteThemselvesMessage);
    }

    public void UpdateUser(int id, string name, string lastname, string email, string? performedByEmail = null)
    {
        if (id == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeModifiedMessage);

        User? userWithEmail = _userRepository.GetByEmail(email);

        if (userWithEmail is not null && userWithEmail.idUser != id)
            throw new UserServicesException(EmailAlreadyExistsMessage);

        User user = GetUserById(id);

        user.UpdatePersonalData(name, lastname, email);

        _userRepository.Update(user);

        _auditLogService.LogPerformedBy(
            AuditActions.UpdateUser,
            $"UserId={user.idUser} updated.",
            performedByEmail,
            nameof(User),
            user.idUser
        );
    }

    public void ChangePassword(int userId, string currentPassword, string newPassword)
    {
        if (userId == SystemAdminUserId)
            throw new UserServicesException(SystemAdminCannotBeModifiedMessage);

        User user = GetUserById(userId);

        user.ChangePassword(currentPassword, newPassword);
        _userRepository.Update(user);
    }

    public void AssignRole(int userId, UserRoles role, string? performedByEmail = null)
    {
        User user = GetUserById(userId);

        user.AddRole(role);

        _userRepository.Update(user);

        _auditLogService.LogPerformedBy(
            AuditActions.AssignUserRole,
            $"Role={role} assigned to UserId={user.idUser}.",
            performedByEmail,
            nameof(User),
            user.idUser
        );
    }

    public void ToggleRole(int userId, UserRoles role, string? performedByEmail = null)
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

            _auditLogService.LogPerformedBy(
                AuditActions.RemoveUserRoles,
                $"Role={role} removed from UserId={user.idUser}.",
                performedByEmail,
                nameof(User),
                user.idUser
            );
        }
        else
        {
            user.AddRole(role);

            _auditLogService.LogPerformedBy(
                AuditActions.AssignUserRole,
                $"Role={role} assigned to UserId={user.idUser}.",
                performedByEmail,
                nameof(User),
                user.idUser
            );
        }

        _userRepository.Update(user);
    }
}
