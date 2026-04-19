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

    
}