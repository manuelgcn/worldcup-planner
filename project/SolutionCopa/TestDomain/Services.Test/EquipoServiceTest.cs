using ProjectCopa;

namespace Services.Test;

[TestClass]
public sealed class EquipoServiceTest
{
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
        const int uefaQuota = 16;
        var service = CrearServicio();

        CargarEquipos(service, uefaQuota, Confederacion.UEFA, 1000);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("UEFA Team 17", 1200, Confederacion.UEFA)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondConmebolQuotaThrowsInvalidOperationException()
    {
        const int conmebolQuota = 7;
        var service = CrearServicio();

        CargarEquipos(service, conmebolQuota, Confederacion.CONMEBOL, 1400);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("CONMEBOL Team 8", 1500, Confederacion.CONMEBOL)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondConcacafQuotaThrowsInvalidOperationException()
    {
        const int concacafQuota = 7;
        var service = CrearServicio();

        CargarEquipos(service, concacafQuota, Confederacion.CONCACAF, 1600);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("CONCACAF Team 8", 1700, Confederacion.CONCACAF)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondCafQuotaThrowsInvalidOperationException()
    {
        const int cafQuota = 9;
        var service = CrearServicio();

        CargarEquipos(service, cafQuota, Confederacion.CAF, 1800);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("CAF Team 10", 1900, Confederacion.CAF)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondAfcQuotaThrowsInvalidOperationException()
    {
        const int afcQuota = 8;
        var service = CrearServicio();

        CargarEquipos(service, afcQuota, Confederacion.AFC, 2000);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("AFC Team 9", 2100, Confederacion.AFC)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondOfcQuotaThrowsInvalidOperationException()
    {
        const int ofcQuota = 1;
        var service = CrearServicio();

        CargarEquipos(service, ofcQuota, Confederacion.OFC, 2200);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CrearEquipo("OFC Team 2", 2300, Confederacion.OFC)));
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
}


