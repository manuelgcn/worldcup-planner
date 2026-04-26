using ProjectCopa;

namespace Team.Tests;

[TestClass]
public sealed class TeamTests
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        
        Assert.AreEqual("Argentina", equipo.Nombre);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidRankingFifa()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFicticioAndConfederacion()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

       
        Assert.IsFalse(equipo.EsFicticio);
        Assert.AreEqual(Confederation.CONMEBOL, equipo.Confederation);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsEmpty()
    {
        
        const int rankingFifa = 2400;

       
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(string.Empty, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsNull()
    {
        
        const int rankingFifa = 2400;

        
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(null!, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfOneCharacter()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("A", rankingFifa, false, Confederation.CONMEBOL);

      
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
        
        const int rankingFifa = 2400;
        var nombre = new string('A', 61);

      
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(nombre, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationAfc()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.AFC);

       
        Assert.AreEqual(Confederation.AFC, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationCaf()
    {
       
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CAF);

       
        Assert.AreEqual(Confederation.CAF, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationConcacaf()
    {
      
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONCACAF);

        
        Assert.AreEqual(Confederation.CONCACAF, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationOfc()
    {
        
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.OFC);

        
        Assert.AreEqual(Confederation.OFC, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationUefa()
    {
       
        const int rankingFifa = 2400;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.UEFA);

        
        Assert.AreEqual(Confederation.UEFA, equipo.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithMinimumValidRankingFifa()
    {
       
        const int rankingFifa = 300;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamIsCreatedWithMaximumValidRankingFifa()
    {
       
        const int rankingFifa = 2500;
        var equipo = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);

        
        Assert.AreEqual(rankingFifa, equipo.RankingFifa);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenRankingFifaBelowMinimum()
    {
       
        const int rankingFifa = 299;

       
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenRankingFifaAboveMaximum()
    {
       
        const int rankingFifa = 2501;

       
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }
}
