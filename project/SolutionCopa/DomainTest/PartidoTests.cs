using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;
using Moq;

namespace DomainTest;

[TestClass]
public class PartidoTests
{

    private Mock<IEquipo> _equipoLocal = null!;
    private Mock<IEquipo> _equipoVisita = null!;
    private Mock<IEstadio> _estadio = null!;
    private Partido _partido = null!;

    private static readonly DateTime FechaPartido = new DateTime(2022, 11, 21);

    private Partido CrearPartidoConDatos()
    {
        return new Partido(1, FechaPartido, _equipoLocal.Object, _equipoVisita.Object, _estadio.Object);
    }
    
    [TestInitialize]
    public void Setup()
    {
        _equipoLocal = new Mock<IEquipo>();
        _equipoVisita = new Mock<IEquipo>();
        _estadio = new Mock<IEstadio>();
        _partido = new Partido();
    }

    [TestMethod]
    public void TestCreationAccurateOfPartidoSinDatos()
    {
        Assert.IsNotNull(_partido);
    }

    [TestMethod]
    public void TestPartidoCanGetIdPartido()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreEqual(1, _partido.IdPartido);
    }

    [TestMethod]
    public void TestPartidoCanGetFechaPartido()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreEqual(FechaPartido, _partido.FechaPartido);
    }

    [TestMethod]
    public void TestPartidoCanGetEquipoLocal()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreSame(_equipoLocal.Object, _partido.EquipoLocal);
    }

    [TestMethod]
    public void TestPartidoCanGetEquipoVisita()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreSame(_equipoVisita.Object, _partido.EquipoVisita);
    }

    [TestMethod]
    public void TestPartidoCanGetGolLocal()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreEqual(0, _partido.GolLocal);
    }

    [TestMethod]
    public void TestPartidoCanGetGolVisita()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreEqual(0, _partido.GolVisita);
    }

    [TestMethod]
    public void TestPartidoCanGetEstadio()
    {
        _partido = CrearPartidoConDatos();
        Assert.AreSame(_estadio.Object, _partido.Estadio);
    }
    
    [TestMethod]
    public void TestPartidoRechazaMismoEquipoLocalYVisita()
    {
        var mismoEquipo = new Mock<IEquipo>();
        Assert.ThrowsException<MatchException>(() =>
            new Partido(1, FechaPartido, mismoEquipo.Object, mismoEquipo.Object, _estadio.Object));
    }
}