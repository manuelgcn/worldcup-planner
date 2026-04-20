using ProjectCopa;
using ProjectCopa.Interfaces;
using Moq;

namespace DomainTest;

[TestClass]
public class PartidoTests
{

    private Mock<IEquipo> equipoLocal = null!;
    private Mock<IEquipo> equipoVisita = null!;
    private Mock<IEstadio> estadio = null!;
    private Partido partido = null!;
    
    [TestInitialize]
    public void Setup()
    {
        equipoLocal = new Mock<IEquipo>();
        equipoVisita = new Mock<IEquipo>();
        estadio = new Mock<IEstadio>();
        partido = new Partido();
    }

    [TestMethod]
    public void TestCreationAccurateOfPartidoSinDatos()
    {
        Assert.IsNotNull(partido);
    }

    [TestMethod]
    public void TestPartidoCanGetIdPartido()
    {
        partido = new Partido(1, new DateTime(2022, 11, 21), equipoLocal.Object, equipoVisita.Object, 4, 1, estadio.Object);
        Assert.AreEqual(1, partido.idPartido);
    }
}