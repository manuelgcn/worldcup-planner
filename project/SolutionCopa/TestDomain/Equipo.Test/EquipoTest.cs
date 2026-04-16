using ProjectCopa;

namespace Equipo.Test;

[TestClass]
public sealed class EquipoTest
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        
        var equipo = new ProjectCopa.Equipo("Argentina", 2400, false, ProjectCopa.Confederacion.CONMEBOL);

        Assert.AreEqual("Argentina", equipo.Nombre);
    }
}