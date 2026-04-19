using ProjectCopa;
using Services;

namespace Services.Test;

[TestClass]
public sealed class EquipoServiceTest
{
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
}

