using DataAccess.Repositories;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class TeamRepositoryListTests
{
    private TeamRepositoryList _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new TeamRepositoryList();
    }

    [TestMethod]
    public void AddStoresTeamAndGetByIdReturnsIt()
    {
        Team team = CreateTeam(1, "Argentina", Confederation.CONMEBOL);

        _repository.Add(team);

        Team? storedTeam = _repository.GetById(1);
        Assert.AreSame(team, storedTeam);
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
        Assert.AreSame(updatedTeam, storedTeam);
    }

    [TestMethod]
    public void UpdateAddsTeamWhenIdDoesNotExist()
    {
        Team team = CreateTeam(3, "Uruguay", Confederation.CONMEBOL);

        _repository.Update(team);

        Assert.AreSame(team, _repository.GetById(3));
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

        Assert.AreSame(team, storedTeam);
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
    public void GetAllReturnsAllStoredTeams()
    {
        Team firstTeam = CreateTeam(7, "Team 1", Confederation.UEFA);
        Team secondTeam = CreateTeam(8, "Team 2", Confederation.CAF);
        _repository.Add(firstTeam);
        _repository.Add(secondTeam);

        IReadOnlyCollection<Team> teams = _repository.GetAll();

        Assert.AreEqual(2, teams.Count);
        Assert.IsTrue(teams.Contains(firstTeam));
        Assert.IsTrue(teams.Contains(secondTeam));
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

    private static Team CreateTeam(int id, string name, Confederation confederation)
    {
        return new Team(id, name, 1000 + id, false, confederation);
    }
}
