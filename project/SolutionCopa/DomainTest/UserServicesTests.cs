using ProjectCopa;
using Services;

namespace DomainTest;
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
    public void CanGetUserByEmail()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
        service.AddUser(user);

        User result = service.GetUserByEmail("juan@mail.com");

        Assert.AreEqual(user, result);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GetUserByEmailThrowsIfNotFound()
    {
        service.GetUserByEmail("noexiste@mail.com");
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
    public void CanRemoveUser()
    {
        User user = new User(1, "Juan", "Perez", "juan@mail.com", "Pass123@", new DateTime(1990, 5, 15) );
        service.AddUser(user);
        service.RemoveUser(1);
        Assert.AreEqual(0, service.GetAllUsers().Count);
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CannotRemoveNonExistingUser()
    {
        service.RemoveUser(99);
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
    [ExpectedException(typeof(ArgumentException))]
    public void CannotUpdateUserThatDoesNotExist()
    {
        service.UpdateUser(99, "Ana", "Lopez", "ana@mail.com");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
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

        service.UpdateUser(2, "Ana", "Lopez", "juan@mail.com");
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

    
}