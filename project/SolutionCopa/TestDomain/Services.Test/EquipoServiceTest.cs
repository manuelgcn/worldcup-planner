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
}

