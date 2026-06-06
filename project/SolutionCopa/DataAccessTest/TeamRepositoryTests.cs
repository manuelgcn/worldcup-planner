using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class TeamRepositoryTests
{
    private TeamRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Contextos efimeros sobre una BD InMemory aislada por test.
        _repository = new TeamRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresTeamAndGetByIdReturnsIt()
    {
        Team team = CreateTeam(1, "Argentina", Confederation.CONMEBOL);

        _repository.Add(team);

        Team? storedTeam = _repository.GetById(1);
        Assert.IsNotNull(storedTeam);
        AssertSameValues(team, storedTeam!);
    }

    [TestMethod]
    public void UpdateReplacesExistingTeamWithSameId()
    {
        Team originalTeam = CreateTeam(2, "Brazil", Confederation.CONMEBOL);
        Team updatedTeam = CreateTeam(2, "Brasil", Confederation.CONMEBOL);
        _repository.Add(originalTeam);

        _repository.Update(updatedTeam);

        Team? storedTeam = _repository.GetById(2);
        Assert.IsNotNull(storedTeam);
        Assert.AreEqual("Brasil", storedTeam!.Name);
        AssertSameValues(updatedTeam, storedTeam);
    }

    [TestMethod]
    public void UpdateAddsTeamWhenIdDoesNotExist()
    {
        Team team = CreateTeam(3, "Uruguay", Confederation.CONMEBOL);

        _repository.Update(team);

        Team? storedTeam = _repository.GetById(3);
        Assert.IsNotNull(storedTeam);
        AssertSameValues(team, storedTeam!);
    }

    [TestMethod]
    public void DeleteRemovesStoredTeam()
    {
        Team team = CreateTeam(4, "Chile", Confederation.CONMEBOL);
        _repository.Add(team);

        _repository.Delete(team);

        Assert.IsNull(_repository.GetById(4));
    }

    [TestMethod]
    public void GetByNameReturnsStoredTeam()
    {
        Team team = CreateTeam(5, "Paraguay", Confederation.CONMEBOL);
        _repository.Add(team);

        Team? storedTeam = _repository.GetByName("Paraguay");

        Assert.IsNotNull(storedTeam);
        AssertSameValues(team, storedTeam!);
    }

    [TestMethod]
    public void ExistsByNameReturnsTrueWhenTeamIsStored()
    {
        Team team = CreateTeam(6, "Ecuador", Confederation.CONMEBOL);
        _repository.Add(team);

        bool exists = _repository.ExistsByName("Ecuador");

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void ExistsByNameReturnsFalseWhenTeamIsNotStored()
    {
        bool exists = _repository.ExistsByName("Nonexistent");

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredTeams()
    {
        Team firstTeam = CreateTeam(7, "Team 1", Confederation.UEFA);
        Team secondTeam = CreateTeam(8, "Team 2", Confederation.CAF);
        _repository.Add(firstTeam);
        _repository.Add(secondTeam);

        IReadOnlyCollection<Team> teams = _repository.GetAll();

        Assert.AreEqual(2, teams.Count);
        Assert.IsTrue(teams.Any(t => t.Id == 7 && t.Name == "Team 1"));
        Assert.IsTrue(teams.Any(t => t.Id == 8 && t.Name == "Team 2"));
    }

    [TestMethod]
    public void CountByConfederationCountsMatchingTeams()
    {
        _repository.Add(CreateTeam(9, "Team 3", Confederation.UEFA));
        _repository.Add(CreateTeam(10, "Team 4", Confederation.UEFA));
        _repository.Add(CreateTeam(11, "Team 5", Confederation.CONMEBOL));

        int count = _repository.CountByConfederation(Confederation.UEFA);

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void AddPersistsTeamRetrievableFromNewContext()
    {
        Team team = CreateTeam(1, "Argentina", Confederation.CONMEBOL);

        _repository.Add(team);                  // SaveChanges interno (autosave por operacion)

        Team? stored = _repository.GetById(1);  // contexto nuevo: el dato viene de la "BD"
        Assert.IsNotNull(stored);
        Assert.AreEqual("Argentina", stored!.Name);
        Assert.AreEqual(Confederation.CONMEBOL, stored.Confederation);
    }
    
    
    [TestMethod]
    public void AddStoresTeamFlagAndGetByIdReturnsIt()
    {
        string validBase64Flag = "dGVzdA==";

        Team team = new Team(
            12,
            "Uruguay",
            2112,
            false,
            Confederation.CONMEBOL,
            validBase64Flag
        );

        _repository.Add(team);

        Team? storedTeam = _repository.GetById(12);

        Assert.IsNotNull(storedTeam);
        Assert.AreEqual(validBase64Flag, storedTeam!.FlagBase64);
    }

    
    
    

    private static Team CreateTeam(int id, string name, Confederation confederation)
    {
        return new Team(id, name, 1000 + id, false, confederation);
    }

    private static void AssertSameValues(Team expected, Team actual)
    {
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.FifaRanking, actual.FifaRanking);
        Assert.AreEqual(expected.IsFictitious, actual.IsFictitious);
        Assert.AreEqual(expected.Confederation, actual.Confederation);
    }
    
    
}
