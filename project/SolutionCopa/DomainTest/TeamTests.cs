using ProjectCopa;

namespace DomainTest;

[TestClass]
public sealed class TeamTests
{
    [TestMethod]
    public void TeamIsCreatedWithValidName()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual("Argentina", team.Name);
    }

    [TestMethod]
    public void TeamHasAnId()
    {
        Team team = new Team(1, "Argentina", 2400, false, Confederation.CONMEBOL);

        Assert.AreEqual(1, team.Id);
    }

    [TestMethod]
    public void TeamHasOnlyOnePublicConstructor()
    {
        Assert.AreEqual(1, typeof(Team).GetConstructors().Length);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFifaRanking()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamIsCreatedWithValidFictitiousFlagAndConfederation()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.IsFalse(team.IsFictitious);
        Assert.AreEqual(Confederation.CONMEBOL, team.Confederation);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsEmpty()
    {
        const int rankingFifa = 2400;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new Team(1, string.Empty, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenNameIsNull()
    {
        const int rankingFifa = 2400;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new Team(1, null!, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfOneCharacter()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "A", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual("A", team.Name);
    }

    [TestMethod]
    public void TeamIsCreatedWithNameOfSixtyCharacters()
    {
        // Arrange
        const int rankingFifa = 2400;
        string name = new string('A', 60);
        Team team = new Team(1, name, rankingFifa, false, Confederation.CONMEBOL);

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
            _ = new Team(1, nombre, rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationAfc()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.AFC);
        Assert.AreEqual(Confederation.AFC, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationCaf()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CAF);
        Assert.AreEqual(Confederation.CAF, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationConcacaf()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONCACAF);
        Assert.AreEqual(Confederation.CONCACAF, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationOfc()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.OFC);
        Assert.AreEqual(Confederation.OFC, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithConfederationUefa()
    {
        const int rankingFifa = 2400;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.UEFA);
        Assert.AreEqual(Confederation.UEFA, team.Confederation);
    }

    [TestMethod]
    public void TeamIsCreatedWithMinimumValidFifaRanking()
    {
        const int rankingFifa = 300;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamIsCreatedWithMaximumValidFifaRanking()
    {
        const int rankingFifa = 2500;
        Team team = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        Assert.AreEqual(rankingFifa, team.FifaRanking);
    }

    [TestMethod]
    public void TeamCreationThrowsWhenFifaRankingIsBelowMinimum()
    {
        const int rankingFifa = 299;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenFifaRankingIsAboveMaximum()
    {
        const int rankingFifa = 2501;
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new Team(1, "Argentina", rankingFifa, false, Confederation.CONMEBOL);
        });
    }

    [TestMethod]
    public void TeamCreationThrowsWhenIdIsZero()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = new ProjectCopa.Team(0, "Argentina", 2400, false, Confederation.CONMEBOL);
        });
    }


    [TestMethod]
    public void CanCreateTeamWithoutFlag_DefaultFlagIsNull()
    {
        Team team = new Team(
            1,
            "Uruguay",
            2100,
            false,
            Confederation.CONMEBOL
        );

        Assert.IsNull(team.FlagBase64);
    }
    
    
    [TestMethod]
    public void CanCreateTeamWithValidBase64Flag()
    {
        string validBase64Flag = "dGVzdA==";

        Team team = new Team(
            1,
            "Uruguay",
            2100,
            false,
            Confederation.CONMEBOL,
            validBase64Flag
        );

        Assert.AreEqual(validBase64Flag, team.FlagBase64);
    }
    
    
    [TestMethod]
    public void CannotCreateTeamWithInvalidBase64Flag()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Team(
                1,
                "Uruguay",
                2100,
                false,
                Confederation.CONMEBOL,
                "esto-no-es-base64"
            )
        );
    }

    
    
    [TestMethod]
    public void CreateTeamWithEmptyFlag_StoresNull()
    {
        Team team = new Team(
            1,
            "Uruguay",
            2100,
            false,
            Confederation.CONMEBOL,
            ""
        );

        Assert.IsNull(team.FlagBase64);
    }

    
    
    [TestMethod]
    public void UpdateFlagWithValidBase64_UpdatesFlag()
    {
        Team team = new Team(
            1,
            "Uruguay",
            2100,
            false,
            Confederation.CONMEBOL
        );

        string validBase64Flag = "dGVzdA==";

        team.UpdateFlag(validBase64Flag);

        Assert.AreEqual(validBase64Flag, team.FlagBase64);
    }

    

}
