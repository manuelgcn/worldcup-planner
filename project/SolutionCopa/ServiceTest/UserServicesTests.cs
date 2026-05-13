using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;
[TestClass]
public class UserServicesTests
{
    
    private UserServices service;

    
    [TestInitialize]
    public void Setup()
    {
        service = new UserServices();
    }
    
    [TestMethod]
    public void CanAddUser()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
        service.AddUser(user);
        Assert.AreEqual(1, service.GetAllUsers().Count);
    }

    [TestMethod]
    public void AddUser_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(AuditLogService.CreateInstance(repository, 1));
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        auditedService.AddUser(user);

        List<AuditLog> logs = repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE_USER", logs[0].Action);
    }

    [TestMethod]
    public void UpdateUser_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(AuditLogService.CreateInstance(repository, 1));
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.UpdateUser(1, "Juan Carlos", "Perez", "juan.c@mail.com");

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == "UPDATE_USER"));
    }

    [TestMethod]
    public void RemoveUser_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(AuditLogService.CreateInstance(repository, 1));
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.RemoveUser(1);

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == "DELETE_USER"));
    }

    [TestMethod]
    public void AssignRole_LogsAuditEntry()
    {
        MockAuditLogRepository repository = new MockAuditLogRepository();
        UserServices auditedService = new UserServices(AuditLogService.CreateInstance(repository, 1));
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        auditedService.AddUser(user);

        auditedService.AssignRole(1, UserRoles.Admin);

        Assert.IsTrue(repository.GetAll().Any(log => log.Action == "ASSIGN_USER_ROLE"));
    }
    
    
    [TestMethod]
    public void CanGetUserByEmail()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
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
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
        service.AddUser(user);

        User result = service.GetUserById(1);

        Assert.AreEqual(user, result);
    }

    [TestMethod]
    public void AddUserThrowsWhenEmailAlreadyExists()
    {
        User firstUser = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = new User(2, "Ana", "Lopez", "juan@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<ArgumentException>(() => service.AddUser(secondUser));
    }

    [TestMethod]
    public void AddUserThrowsWhenIdAlreadyExists()
    {
        User firstUser = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));
        User secondUser = new User(1, "Ana", "Lopez", "ana@mail.com", "Pass123@", new DateTime(1995, 3, 10));
        service.AddUser(firstUser);

        Assert.ThrowsException<ArgumentException>(() => service.AddUser(secondUser));
    }

    
    [TestMethod]
    public void CanRemoveUser()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
        service.AddUser(user);
        service.RemoveUser(1);
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
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15));

        service.AddUser(user);

        service.UpdateUser(1, "Juan Carlos", "Perez", "juan.c@mail.com");

        User updated = service.GetUserById(1);

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
        User user1 = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        User user2 = new User(
            2,
            "Ana",
            "Lopez",
            "ana@mail.com",
            "Pass123@",
            new DateTime(1995, 3, 10)
        );

        service.AddUser(user1);
        service.AddUser(user2);

        Assert.ThrowsException<ArgumentException>(() =>
            service.UpdateUser(2, "Ana", "Lopez", "juan@mail.com"));
    }

    
    [TestMethod]
    public void AdminCanAssignRoleToUser()
    {
        User user = new User(
            1,
            "Juan",
            "Perez",
            "juan@mail.com",
            "Pass123@",
            new DateTime(1990, 5, 15)
        );

        service.AddUser(user);

        service.AssignRole(1, UserRoles.Admin);

        User updated = service.GetUserById(1);

        Assert.IsTrue(updated.Roles.Contains(UserRoles.Admin));
    }

    
   
    [TestMethod]
    public void CannotAssignRoleToNonExistingUser()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            service.AssignRole(99, UserRoles.Editor));
    }

    
}
