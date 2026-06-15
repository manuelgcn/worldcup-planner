using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class UserServicesTests
{
    private UserServices service;
    private MockUserRepository repository;

    [TestInitialize]
    public void Setup()
    {
        repository = new MockUserRepository();
        service = new UserServices(repository, new MockAuditLogService());
    }

    private static User BuildUser(
        int id,
        string name,
        string lastname,
        string email,
        string password,
        DateTime dateOfBirth,
        UserRoles role = UserRoles.Editor)
    {
        User user = new User(id, name, lastname, email, password, dateOfBirth);
        user.AddRole(role);
        return user;
    }

    [TestMethod]
    public void RegisterUser_BuildsDomainUserFromRequestAndPersistsIt()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 5,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        });

        User stored = service.GetUserById(5);
        Assert.AreEqual("ana@mail.com", stored.email);
        // El registro publico siempre crea Periodista, sin importar el rol del request.
        Assert.IsTrue(stored.HasRole(UserRoles.Periodista));
    }

    [TestMethod]
    public void RegisterUser_IgnoresRequestedPrivilegedRole_AndForcesPeriodista()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 9,
            Name = "Eva",
            Lastname = "Diaz",
            Email = "eva@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Admin
        });

        User stored = service.GetUserById(9);
        Assert.IsTrue(stored.HasRole(UserRoles.Periodista));
        Assert.IsFalse(stored.HasRole(UserRoles.Admin));
        Assert.IsFalse(stored.HasRole(UserRoles.Editor));
        Assert.AreEqual(1, stored.Roles.Count);
    }

    [TestMethod]
    public void RegisterUser_WithInvalidEmail_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => service.RegisterUser(new CreateUserRequestDto
        {
            Id = 6,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "correo-invalido",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        }));
    }

    [TestMethod]
    public void RegisterUser_WithDuplicateEmail_ThrowsUserServicesException()
    {
        service.RegisterUser(new CreateUserRequestDto
        {
            Id = 7,
            Name = "Ana",
            Lastname = "Lopez",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1992, 4, 10),
            InitialRole = UserRoles.Editor
        });

        Assert.ThrowsException<UserServicesException>(() => service.RegisterUser(new CreateUserRequestDto
        {
            Id = 8,
            Name = "Otra",
            Lastname = "Persona",
            Email = "ana@mail.com",
            Password = "Pass123@",
            DateOfBirth = new DateTime(1990, 1, 1),
            InitialRole = UserRoles.Editor
        }));
    }

    [TestMethod]
    public void CanAddUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);
        Assert.AreEqual(1, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void AddUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user);

        List<AuditLog> logs = auditRepo.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE_USER", logs[0].Action);
    }

    [TestMethod]
    public void UpdateUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "UPDATE_USER"));
    }

    [TestMethod]
    public void AddUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "CREATE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void UpdateUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com", performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "UPDATE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void RemoveUser_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.RemoveUser(2, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "DELETE_USER");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void AssignRole_RecordsPerformingActor()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.AssignRole(2, UserRoles.Admin, performedByEmail: "admin@copa.com");

        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "ASSIGN_USER_ROLE");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void Authenticate_WithValidCredentials_ReturnsUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User authenticated = service.Authenticate("juan@mail.com", "Pass123@");

        Assert.AreEqual(user.idUser, authenticated.idUser);
    }

    [TestMethod]
    public void Authenticate_WithWrongPassword_Throws()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() => service.Authenticate("juan@mail.com", "Wrong123@"));
    }

    [TestMethod]
    public void Authenticate_WithUnknownEmail_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() => service.Authenticate("nobody@mail.com", "Pass123@"));
    }

    [TestMethod]
    public void RemoveUser_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.RemoveUser(2);

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "DELETE_USER"));
    }

    [TestMethod]
    public void AssignRole_LogsAuditEntry()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.AssignRole(1, UserRoles.Admin);

        Assert.IsTrue(auditRepo.GetAll().Any(log => log.Action == "ASSIGN_USER_ROLE"));
    }

    [TestMethod]
    public void CanGetUserByEmail()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User result = service.GetUserByEmail("juan@mail.com");

        Assert.AreEqual(user, result);
    }

    [TestMethod]
    public void GetUserByEmailThrowsIfNotFound()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.GetUserByEmail("noexiste@mail.com"));
    }

    [TestMethod]
    public void CanGetUserById()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        User result = service.GetUserById(1);

        Assert.AreEqual(user, result);
    }

    [TestMethod]
    public void AddUserThrowsWhenEmailAlreadyExists()
    {
        User firstUser = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = BuildUser(2, "Ana", "Lopez", "juan@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void AddUserThrowsWhenIdAlreadyExists()
    {
        User firstUser = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = BuildUser(1, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void CanRemoveUser()
    {
        // idUser != 1: el id 1 esta reservado al admin del sistema y la regla de negocio impide eliminarlo.
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);
        service.RemoveUser(2);
        Assert.AreEqual(0, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void CannotRemoveNonExistingUser()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveUser(99));
    }

    [TestMethod]
    public void CanUpdateUserData()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        service.AddUser(user);

        service.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");

        User updated = service.GetUserById(2);

        Assert.AreEqual("Juan Carlos", updated.name);
        Assert.AreEqual("juan.c@mail.com", updated.email);
    }

    [TestMethod]
    public void CannotUpdateUserThatDoesNotExist()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(99, "Ana", "Lopez", "ana@mail.com"));
    }

    [TestMethod]
    public void CannotUpdateUserWithDuplicatedEmail()
    {
        User user1 = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User user2 = BuildUser(2, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));

        service.AddUser(user1);
        service.AddUser(user2);

        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(2, "Ana", "Lopez", "juan@mail.com"));
    }

    [TestMethod]
    public void AdminCanAssignRoleToUser()
    {
        User user = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        service.AddUser(user);

        service.AssignRole(1, UserRoles.Admin);

        User updated = service.GetUserById(1);

        Assert.IsTrue(updated.HasRole(UserRoles.Admin));
    }

    [TestMethod]
    public void CannotAssignRoleToNonExistingUser()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.AssignRole(99, UserRoles.Editor));
    }

    [TestMethod]
    public void GetAllUsers_ReturnsAllRegisteredUsersInInsertionOrder()
    {
        User first = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User second = BuildUser(2, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));

        service.AddUser(first);
        service.AddUser(second);

        List<User> all = service.GetAllUsers().ToList();

        Assert.AreEqual(2, all.Count);
        Assert.AreEqual(1, all[0].idUser);
        Assert.AreEqual(2, all[1].idUser);
    }

    [TestMethod]
    public void FullAbmLifecycle_ExecutesEveryAuditAction()
    {
        MockUserRepository userRepo = new MockUserRepository();
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();

        UserServices auditedService = new UserServices(
            userRepo,
            AuditLogService.CreateInstance(auditRepo)
        );

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user);
        auditedService.UpdateUser(2, "Juan Carlos", "Perez", "juan.c@mail.com");
        auditedService.AssignRole(2, UserRoles.Admin);
        auditedService.RemoveUser(2);

        List<string> actions = auditRepo.GetAll().Select(log => log.Action).ToList();

        Assert.IsTrue(actions.Contains("CREATE_USER"));
        Assert.IsTrue(actions.Contains("UPDATE_USER"));
        Assert.IsTrue(actions.Contains("ASSIGN_USER_ROLE"));
        Assert.IsTrue(actions.Contains("DELETE_USER"));
        Assert.AreEqual(0, auditedService.GetAllUsers().Count);
    }

    [TestMethod]
    public void AddUser_WithoutAnyRole_Throws()
    {
        User user = new User(3, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        Assert.ThrowsException<UserServicesException>(() => service.AddUser(user));
    }

    [TestMethod]
    public void RemoveUser_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() => service.RemoveUser(1));
    }

    [TestMethod]
    public void UpdateUser_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.UpdateUser(1, "Ana", "Lopez", "ana@mail.com"));
    }

    [TestMethod]
    public void UpdateUser_KeepingSameEmail_Succeeds()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        service.AddUser(user);

        service.UpdateUser(2, "Juan Carlos", "Perez", "juan@mail.com");

        User updated = service.GetUserById(2);
        Assert.AreEqual("Juan Carlos", updated.name);
        Assert.AreEqual("juan@mail.com", updated.email);
    }

    [TestMethod]
    public void RemoveRole_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveRole(1, UserRoles.Editor));
    }

    [TestMethod]
    public void RemoveRole_WhenUserDoesNotHaveRole_DoesNothing()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        service.RemoveRole(2, UserRoles.Admin);

        User unchanged = service.GetUserById(2);
        Assert.IsTrue(unchanged.HasRole(UserRoles.Editor));
        Assert.AreEqual(1, unchanged.Roles.Count);
    }

    [TestMethod]
    public void RemoveRole_WhenUserHasOnlyOneRole_Throws()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() =>
            service.RemoveRole(2, UserRoles.Editor));
    }

    [TestMethod]
    public void RemoveRole_RemovesRoleAndLogsAuditEntry()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);
        auditedService.AssignRole(2, UserRoles.Admin);

        auditedService.RemoveRole(2, UserRoles.Admin, performedByEmail: "admin@copa.com");

        User updated = auditedService.GetUserById(2);
        Assert.IsFalse(updated.HasRole(UserRoles.Admin));
        Assert.IsTrue(updated.HasRole(UserRoles.Editor));
        AuditLog log = auditRepo.GetAll().Single(l => l.Action == "REMOVE_ROLES");
        Assert.AreEqual("admin@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void ToggleRole_SystemAdmin_Throws()
    {
        Assert.ThrowsException<UserServicesException>(() =>
            service.ToggleRole(1, UserRoles.Admin));
    }

    [TestMethod]
    public void ToggleRole_WhenUserLacksRole_AddsItAndLogsAssign()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);

        auditedService.ToggleRole(2, UserRoles.Admin);

        Assert.IsTrue(auditedService.GetUserById(2).HasRole(UserRoles.Admin));
        Assert.IsTrue(auditRepo.GetAll().Any(l => l.Action == "ASSIGN_USER_ROLE"));
    }

    [TestMethod]
    public void ToggleRole_WhenUserHasRole_RemovesItAndLogsRemove()
    {
        MockAuditLogRepository auditRepo = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(
            new MockUserRepository(),
            AuditLogService.CreateInstance(auditRepo));

        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        auditedService.AddUser(user);
        auditedService.AssignRole(2, UserRoles.Admin);

        auditedService.ToggleRole(2, UserRoles.Admin);

        Assert.IsFalse(auditedService.GetUserById(2).HasRole(UserRoles.Admin));
        Assert.IsTrue(auditRepo.GetAll().Any(l => l.Action == "REMOVE_ROLES"));
    }

    [TestMethod]
    public void ToggleRole_WhenUserHasOnlyThatRole_Throws()
    {
        User user = BuildUser(2, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15), UserRoles.Editor);
        service.AddUser(user);

        Assert.ThrowsException<UserServicesException>(() =>
            service.ToggleRole(2, UserRoles.Editor));
    }
}
