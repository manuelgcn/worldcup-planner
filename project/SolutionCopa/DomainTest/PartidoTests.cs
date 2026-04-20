using ProjectCopa;

namespace DomainTest;

[TestClass]
public class PartidoTests
{
    [TestMethod]
    public void TestCreationOfPartido()
    {
        var partido = new  Partido();
        Assert.IsNotNull(partido);
    }
    
}