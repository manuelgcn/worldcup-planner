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

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsEmpty()
    {
        // Arrange
        const int rankingFifa = 2400;
        Action act = () =>
            new ProjectCopa.Equipo(string.Empty, rankingFifa, false, Confederacion.CONMEBOL);

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(act);
    }
}