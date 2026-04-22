using ProjectCopa;

namespace Team.Tests;

[TestClass]
public sealed class TeamTests
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual("Argentina", equipo.Nombre);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidRankingFifa()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFicticioAndConfederacion()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.IsFalse(equipo.EsFicticio);
        Assert.AreEqual(Confederation.CONMEBOL, equipo.Confederation);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsEmpty()
    {
        // Arrange
        const int rankingFifa = 2400;

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(string.Empty, rankingFifa, false, Confederation.CONMEBOL);
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
            _ = new ProjectCopa.Team(null!, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfOneCharacter()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("A", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual("A", equipo.Nombre);
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfSixtyCharacters()
    {
        // Arrange
        const int rankingFifa = 2400;
        var nombre = new string('A', 60);
        var equipo = new ProjectCopa.Team(nombre, rankingFifa, false, Confederation.CONMEBOL);

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
            _ = new ProjectCopa.Team(nombre, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationAfc()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.AFC);

        // Assert
        Assert.AreEqual(Confederation.AFC, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationCaf()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CAF);

        // Assert
        Assert.AreEqual(Confederation.CAF, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationConcacaf()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONCACAF);

        // Assert
        Assert.AreEqual(Confederation.CONCACAF, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationOfc()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.OFC);

        // Assert
        Assert.AreEqual(Confederation.OFC, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationUefa()
    {
        // Arrange
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.UEFA);

        // Assert
        Assert.AreEqual(Confederation.UEFA, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithMinimumValidRankingFifa()
    {
        // Arrange
        const int rankingFifa = 300;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamIsCreatedWithMaximumValidRankingFifa()
    {
        // Arrange
        const int rankingFifa = 2500;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenRankingFifaBelowMinimum()
    {
        // Arrange
        const int rankingFifa = 299;

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenRankingFifaAboveMaximum()
    {
        // Arrange
        const int rankingFifa = 2501;

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }
}
