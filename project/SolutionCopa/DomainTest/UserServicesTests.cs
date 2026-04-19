using Services;

namespace DomainTest;
[TestClass]
public class UserServicesTests
{
    
    private UserServices service;

    
    [TestInitialize]
    public void Setup()
    {
        service = new UserService();
    }

}