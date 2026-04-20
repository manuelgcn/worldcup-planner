using ProjectCopa;

namespace DomainTest;

[TestClass]
public class PartidoTests
{

    private Partido partido;
    
    [TestInitialize]
    public void Setup()
    {
        partido = new  Partido();
    }

    [TestMethod]
    public void TestCreationAccurateOfPartidoSinDatos()
    {
        Assert.IsNotNull(partido);
    }

    [TestMethod]
    public void TestCreationGetIdPartido()
    {
        partido = new Partido(1, new DateTime(2022, 11, 21), "Uruguay", "Argentina", 4, 1, "Al Bayt Stadium");
        Assert.AreEqual(1, partido.idPartido);
    }
}