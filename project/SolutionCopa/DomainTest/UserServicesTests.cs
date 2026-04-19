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
    

    
}