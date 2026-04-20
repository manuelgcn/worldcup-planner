using ProjectCopa;

namespace Services.Test;

[TestClass]
public sealed class EquipoServiceTest
{
    private const int RankingBaseUefa = 1000;
    private const int RankingBaseConmebol = 1400;
    private const int RankingBaseConcacaf = 1600;
    private const int RankingBaseCaf = 1800;
    private const int RankingBaseAfc = 2000;
    private const int RankingBaseOfc = 2200;

    private static readonly Confederacion[] Confederaciones =
    [
        Confederacion.AFC,
        Confederacion.CAF,
        Confederacion.CONCACAF,
        Confederacion.CONMEBOL,
        Confederacion.OFC,
        Confederacion.UEFA
    ];

    [TestMethod]
    public void RegisteringTeamWithDuplicateNameThrowsInvalidOperationException()
    {
        var service = CrearServicio();
        var firstTeam = CrearEquipo("Argentina", 2400, Confederacion.CONMEBOL);
        var duplicateTeam = CrearEquipo("Argentina", 2300, Confederacion.CONMEBOL);

        service.AgregarEquipo(firstTeam);

        Assert.ThrowsException<InvalidOperationException>(() => service.AgregarEquipo(duplicateTeam));
    }

    [TestMethod]
    public void RegisteringTeamBeyondUefaQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.UEFA, cuota: 16, RankingBaseUefa, "UEFA Team 17", 1200);
    }

    [TestMethod]
    public void RegisteringTeamBeyondConmebolQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.CONMEBOL, cuota: 7, RankingBaseConmebol, "CONMEBOL Team 8", 1500);
    }

    [TestMethod]
    public void RegisteringTeamBeyondConcacafQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.CONCACAF, cuota: 7, RankingBaseConcacaf, "CONCACAF Team 8", 1700);
    }

    [TestMethod]
    public void RegisteringTeamBeyondCafQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.CAF, cuota: 9, RankingBaseCaf, "CAF Team 10", 1900);
    }

    [TestMethod]
    public void RegisteringTeamBeyondAfcQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.AFC, cuota: 8, RankingBaseAfc, "AFC Team 9", 2100);
    }

    [TestMethod]
    public void RegisteringTeamBeyondOfcQuotaThrowsInvalidOperationException()
    {
        AssertExcesoDeCupo(Confederacion.OFC, cuota: 1, RankingBaseOfc, "OFC Team 2", 2300);
    }

    [TestMethod]
    public void FixtureOrderingWithSameSeedProducesSameOrderWhenRankingHasTies()
    {
        var service = CrearServicio();
        var equipos = Crear48EquiposConEmpatesDeRanking();
        const int semillaFixture = 2026;

        var primerOrden = service.OrdenarParaFixture(equipos, semillaFixture).Select(equipo => equipo.Nombre).ToList();
        var segundoOrden = service.OrdenarParaFixture(equipos, semillaFixture).Select(equipo => equipo.Nombre).ToList();

        Assert.AreEqual(48, primerOrden.Count);
        CollectionAssert.AreEqual(primerOrden, segundoOrden);
    }


    private static IReadOnlyCollection<Equipo> Crear48EquiposConEmpatesDeRanking()
    {
        var equipos = new List<Equipo>(capacity: 48);

        for (var i = 0; i < 48; i++)
        {
            var rankingConEmpate = 2500 - (i / 4) * 10;
            var confederacion = Confederaciones[i % Confederaciones.Length];
            equipos.Add(CrearEquipo($"Team {i + 1}", rankingConEmpate, confederacion));
        }

        return equipos;
    }

    [TestMethod]
    public void FindingTeamByNameReturnsRegisteredTeam()
    {
        var service = CrearServicio();
        var argentina = CrearEquipo("Argentina", 2400, Confederacion.CONMEBOL);
        service.AgregarEquipo(argentina);

        var foundTeam = service.BuscarPorNombre("Argentina");

        Assert.IsNotNull(foundTeam);
        Assert.AreEqual("Argentina", foundTeam.Nombre);
    }

    [TestMethod]
    public void FindingTeamByNameWhenTeamDoesNotExistThrowsInvalidOperationException()
    {
        var service = CrearServicio();

        Assert.ThrowsException<InvalidOperationException>(() => service.BuscarPorNombre("Brazil"));
    }

    [TestMethod]
    public void DeletingRegisteredTeamRemovesItFromService()
    {
        var service = CrearServicio();
        var argentina = CrearEquipo("Argentina", 2400, Confederacion.CONMEBOL);
        service.AgregarEquipo(argentina);

        service.EliminarEquipo("Argentina");

        Assert.ThrowsException<InvalidOperationException>(() => service.BuscarPorNombre("Argentina"));
    }

    private static EquipoService CrearServicio()
    {
        return new EquipoService();
    }

    private static Equipo CrearEquipo(string nombre, int rankingFifa, Confederacion confederacion)
    {
        return new Equipo(nombre, rankingFifa, false, confederacion);
    }

    private static void CargarEquipos(EquipoService service, int cantidad, Confederacion confederacion, int rankingBase)
    {
        for (var i = 1; i <= cantidad; i++)
        {
            service.AgregarEquipo(CrearEquipo($"{confederacion} Team {i}", rankingBase + i, confederacion));
        }
    }

    private static void AssertExcesoDeCupo(
        Confederacion confederacion,
        int cuota,
        int rankingBase,
        string nombreExcedido,
        int rankingExcedido)
    {
        var service = CrearServicio();

        CargarEquipos(service, cuota, confederacion, rankingBase);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo(nombreExcedido, rankingExcedido, confederacion)));
    }
}


