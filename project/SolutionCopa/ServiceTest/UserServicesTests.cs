using ProjectCopa;
using Services;
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
        service = new UserServices(repository);
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
        Assert.ThrowsException<ArgumentException>(() =>
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

        Assert.ThrowsException<ArgumentException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void AddUserThrowsWhenIdAlreadyExists()
    {
        User firstUser = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = BuildUser(1, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<ArgumentException>(() => service.AddUser(secondUser));
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
        Assert.ThrowsException<ArgumentException>(() =>
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
        Assert.ThrowsException<ArgumentException>(() =>
            service.UpdateUser(99, "Ana", "Lopez", "ana@mail.com"));
    }

    [TestMethod]
    public void CannotUpdateUserWithDuplicatedEmail()
    {
        User user1 = BuildUser(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User user2 = BuildUser(2, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));

        service.AddUser(user1);
        service.AddUser(user2);

        Assert.ThrowsException<ArgumentException>(() =>
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
        Assert.ThrowsException<ArgumentException>(() =>
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
}
