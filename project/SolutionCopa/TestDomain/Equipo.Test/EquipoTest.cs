using ProjectCopa;

namespace Equipo.Test;

[TestClass]
public sealed class EquipoTest
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, ProjectCopa.Confederacion.CONMEBOL);

        Assert.AreEqual("Argentina", equipo.Nombre);
    }
}