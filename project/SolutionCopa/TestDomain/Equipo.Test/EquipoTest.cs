using ProjectCopa;

namespace Equipo.Test;

[TestClass]
public sealed class EquipoTest
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.CONMEBOL);

        // Assert
        Assert.AreEqual("Argentina", equipo.Nombre);
    }
}