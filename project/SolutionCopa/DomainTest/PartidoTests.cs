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

    private static readonly DateTime FechaPartido = new DateTime(2022, 11, 21);

    private Partido CrearPartidoConDatos()
    {
        return new Partido(1, FechaPartido, equipoLocal.Object, equipoVisita.Object, 4, 1, estadio.Object);
    }
    
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
        partido = CrearPartidoConDatos();
        Assert.AreEqual(1, partido.idPartido);
    }

    [TestMethod]
    public void TestPartidoCanGetFechaPartido()
    {
        partido = CrearPartidoConDatos();
        Assert.AreEqual(FechaPartido, partido.fechaPartido);
    }

    [TestMethod]
    public void TestPartidoCanGetEquipoLocal()
    {
        partido = CrearPartidoConDatos();
        Assert.AreSame(equipoLocal.Object, partido.equipoLocal);
    }

    [TestMethod]
    public void TestPartidoCanGetEquipoVisita()
    {
        partido = CrearPartidoConDatos();
        Assert.AreSame(equipoVisita.Object, partido.equipoVisita);
    }

    [TestMethod]
    public void TestPartidoCanGetGolLocal()
    {
        partido = CrearPartidoConDatos();
        Assert.AreEqual(4, partido.golLocal);
    }

    [TestMethod]
    public void TestPartidoCanGetGolVisita()
    {
        partido = CrearPartidoConDatos();
        Assert.AreEqual(1, partido.golVisita);
    }

    [TestMethod]
    public void TestPartidoCanGetEstadio()
    {
        partido = CrearPartidoConDatos();
        Assert.AreSame(estadio.Object, partido.estadio);
    }
}