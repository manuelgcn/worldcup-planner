using ProjectCopa;
using Services;

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
        var service = new EquipoService();
        var firstTeam = new Equipo("Argentina", 2400, false, Confederacion.CONMEBOL);
        var duplicateTeam = new Equipo("Argentina", 2300, false, Confederacion.CONMEBOL);

        service.AgregarEquipo(firstTeam);

        Assert.ThrowsException<InvalidOperationException>(() => service.AgregarEquipo(duplicateTeam));
    }

    [TestMethod]
    public void RegisteringTeamBeyondUefaQuotaThrowsInvalidOperationException()
    {
        const int uefaQuota = 16;
        var service = new EquipoService();

        for (var i = 1; i <= uefaQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"UEFA Team {i}", 1000 + i, false, Confederacion.UEFA));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("UEFA Team 17", 1200, false, Confederacion.UEFA)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondConmebolQuotaThrowsInvalidOperationException()
    {
        const int conmebolQuota = 7;
        var service = new EquipoService();

        for (var i = 1; i <= conmebolQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"CONMEBOL Team {i}", 1400 + i, false, Confederacion.CONMEBOL));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("CONMEBOL Team 8", 1500, false, Confederacion.CONMEBOL)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondConcacafQuotaThrowsInvalidOperationException()
    {
        const int concacafQuota = 7;
        var service = new EquipoService();

        for (var i = 1; i <= concacafQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"CONCACAF Team {i}", 1600 + i, false, Confederacion.CONCACAF));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("CONCACAF Team 8", 1700, false, Confederacion.CONCACAF)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondCafQuotaThrowsInvalidOperationException()
    {
        const int cafQuota = 9;
        var service = new EquipoService();

        for (var i = 1; i <= cafQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"CAF Team {i}", 1800 + i, false, Confederacion.CAF));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("CAF Team 10", 1900, false, Confederacion.CAF)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondAfcQuotaThrowsInvalidOperationException()
    {
        const int afcQuota = 8;
        var service = new EquipoService();

        for (var i = 1; i <= afcQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"AFC Team {i}", 2000 + i, false, Confederacion.AFC));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("AFC Team 9", 2100, false, Confederacion.AFC)));
    }

    [TestMethod]
    public void RegisteringTeamBeyondOfcQuotaThrowsInvalidOperationException()
    {
        const int ofcQuota = 1;
        var service = new EquipoService();

        for (var i = 1; i <= ofcQuota; i++)
        {
            service.AgregarEquipo(new Equipo($"OFC Team {i}", 2200 + i, false, Confederacion.OFC));
        }

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(new Equipo("OFC Team 2", 2300, false, Confederacion.OFC)));
    }

    [TestMethod]
    public void FixtureOrderingWithSameSeedProducesSameOrderWhenRankingHasTies()
    {
        var service = new EquipoService();
        var equipos = Crear48EquiposConEmpatesDeRanking();
        const int semillaFixture = 2026;

        var primerOrden = ObtenerNombresOrdenadosParaFixture(service, equipos, semillaFixture);
        var segundoOrden = ObtenerNombresOrdenadosParaFixture(service, equipos, semillaFixture);

        Assert.AreEqual(48, primerOrden.Count);
        CollectionAssert.AreEqual(primerOrden, segundoOrden);
    }

    private static List<string> ObtenerNombresOrdenadosParaFixture(EquipoService service, IReadOnlyCollection<Equipo> equipos, int semillaFixture)
    {
        var metodo = typeof(EquipoService)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "OrdenarParaFixture" && m.GetParameters().Length == 2);

        if (metodo is null)
        {
            Assert.Fail("Expected method EquipoService.OrdenarParaFixture(IReadOnlyCollection<Equipo>, int semillaFixture) was not found.");
        }

        var resultado = metodo!.Invoke(service, [equipos, semillaFixture]);
        if (resultado is null)
        {
            Assert.Fail("OrdenarParaFixture returned null.");
        }

        if (resultado is IEnumerable<Equipo> equiposOrdenados)
        {
            return equiposOrdenados.Select(equipo => equipo.Nombre).ToList();
        }

        var propiedadEquiposOrdenados = resultado!.GetType().GetProperty("EquiposOrdenados");
        if (propiedadEquiposOrdenados?.GetValue(resultado) is IEnumerable<Equipo> equiposDesdePropiedad)
        {
            return equiposDesdePropiedad.Select(equipo => equipo.Nombre).ToList();
        }

        Assert.Fail("OrdenarParaFixture must return IEnumerable<Equipo> or an object with EquiposOrdenados.");
        return [];
    }

    private static IReadOnlyCollection<Equipo> Crear48EquiposConEmpatesDeRanking()
    {
        var equipos = new List<Equipo>(capacity: 48);

        for (var i = 0; i < 48; i++)
        {
            var rankingConEmpate = 2500 - (i / 4) * 10;
            var confederacion = Confederaciones[i % Confederaciones.Length];
            equipos.Add(new Equipo($"Team {i + 1}", rankingConEmpate, false, confederacion));
        }

        return equipos;
    }
}

