using ProjectCopa;

namespace TestDomain;
public class UsuarioTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void UsuarioNotNull()
    {
        Usuario user = new Usuario();
        Assert.IsNotNull(user);
    }
    
    [Test]
    public void UsuarioCanBeInitializedWithName()
    {
        Usuario user = new Usuario("Juan");
        Assert.AreEqual("Juan", user.name);
    }
}