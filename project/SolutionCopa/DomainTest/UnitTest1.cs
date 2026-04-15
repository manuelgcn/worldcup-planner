using System.Reflection.Metadata;
using ProjectCopa;


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
    
    [TestMethod]
    public void UserCanBeInitializedWithName()
    {
        Usuario user = new Usuario("Juan");
        Assert.AreEqual("Juan", user.name);
    }
    
    [TestMethod]
    public void UserCanBeInitializedWithNameAndLastname()
    {
        Usuario user = new Usuario("Juan", "Pérez");
        Assert.AreEqual("Juan", user.name);
        Assert.AreEqual("Pérez", user.lastname);
    }
}