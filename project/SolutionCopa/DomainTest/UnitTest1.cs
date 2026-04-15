using System.Reflection.Metadata;

namespace DomainTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void UserNotNull()
    {
        Usuario user = new Usuario();
        Assert.IsNotNull(user);
    }
}