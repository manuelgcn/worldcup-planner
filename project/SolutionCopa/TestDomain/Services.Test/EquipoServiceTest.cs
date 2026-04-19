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
}

