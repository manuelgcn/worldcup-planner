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
    public void TeamIsCreatedWithValidRankingFifa()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.CONMEBOL);

        // Assert
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFicticioAndConfederacion()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.CONMEBOL);

        // Assert
        Assert.IsFalse(equipo.EsFicticio);
        Assert.AreEqual(Confederacion.CONMEBOL, equipo.Confederacion);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsEmpty()
    {
        // Arrange
        const int rankingFifa = 2400;

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Equipo(string.Empty, rankingFifa, false, Confederacion.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsNull()
    {
        // Arrange
        const int rankingFifa = 2400;

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Equipo(null!, rankingFifa, false, Confederacion.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfOneCharacter()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("A", rankingFifa, false, Confederacion.CONMEBOL);

        // Assert
        Assert.AreEqual("A", equipo.Nombre);
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfSixtyCharacters()
    {
        // Arrange
        const int rankingFifa = 2400;
        var nombre = new string('A', 60);
        var equipo = new ProjectCopa.Equipo(nombre, rankingFifa, false, Confederacion.CONMEBOL);

        // Assert
        Assert.AreEqual(nombre, equipo.Nombre);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameExceedsSixtyCharacters()
    {
        // Arrange
        const int rankingFifa = 2400;
        var nombre = new string('A', 61);

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Equipo(nombre, rankingFifa, false, Confederacion.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederacionAfc()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.AFC);

        // Assert
        Assert.AreEqual(Confederacion.AFC, equipo.Confederacion);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederacionCaf()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.CAF);

        // Assert
        Assert.AreEqual(Confederacion.CAF, equipo.Confederacion);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederacionConcacaf()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.CONCACAF);

        // Assert
        Assert.AreEqual(Confederacion.CONCACAF, equipo.Confederacion);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederacionOfc()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.OFC);

        // Assert
        Assert.AreEqual(Confederacion.OFC, equipo.Confederacion);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederacionUefa()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Equipo("Argentina", rankingFifa, false, Confederacion.UEFA);

        // Assert
        Assert.AreEqual(Confederacion.UEFA, equipo.Confederacion);
    }
}