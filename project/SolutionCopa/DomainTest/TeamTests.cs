using ProjectCopa;

namespace DomainTest;

[TestClass]
public sealed class TeamTests
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual("Argentina", team.Name);
    }

    [TestMethod]
    public void TeamHasAnId()
    {
        var team = new ProjectCopa.Team(1, "Argentina", 2400, false, Confederation.CONMEBOL);

        Assert.AreEqual(1, team.Id);
    }

    [TestMethod]
    public void TeamHasOnlyOnePublicConstructor()
    {
        Assert.AreEqual(1, typeof(ProjectCopa.Team).GetConstructors().Length);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFifaRanking()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFictitiousFlagAndConfederation()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.IsFalse(team.IsFictitious);
        Assert.AreEqual(Confederation.CONMEBOL, team.Confederation);
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
        var team = new ProjectCopa.Team("A", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual("A", team.Name);
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfSixtyCharacters()
    {
        // Arrange
        const int rankingFifa = 2400;
        string name = new string('A', 60);
        var team = new ProjectCopa.Team(name, rankingFifa, false, Confederation.CONMEBOL);

        // Assert
        Assert.AreEqual(name, team.Name);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameExceedsSixtyCharacters()
    {
        const int rankingFifa = 2400;
        string nombre = new string('A', 61);
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(nombre, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationAfc()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.AFC);
        Assert.AreEqual(Confederation.AFC, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationCaf()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CAF);
        Assert.AreEqual(Confederation.CAF, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationConcacaf()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONCACAF);
        Assert.AreEqual(Confederation.CONCACAF, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationOfc()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.OFC);
        Assert.AreEqual(Confederation.OFC, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationUefa()
    {
        const int rankingFifa = 2400;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.UEFA);
        Assert.AreEqual(Confederation.UEFA, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithMinimumValidFifaRanking()
    {
        const int rankingFifa = 300;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamIsCreatedWithMaximumValidFifaRanking()
    {
        const int rankingFifa = 2500;
        var team = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenFifaRankingIsBelowMinimum()
    {
        const int rankingFifa = 299;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenFifaRankingIsAboveMaximum()
    {
        const int rankingFifa = 2501;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team("Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }
}
