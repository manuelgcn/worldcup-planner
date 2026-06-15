using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class GroupRepositoryTests
{
    private InMemoryDbContextFactory _factory = null!;
    private GroupRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new InMemoryDbContextFactory();
        _repository = new GroupRepository(_factory);
    }

    [TestMethod]
    public void AddStoresGroupAndGetByNameReturnsIt()
    {
        _repository.Add(new Group("A"));

        Group? stored = _repository.GetByName("A");

        Assert.IsNotNull(stored);
        Assert.AreEqual("A", stored!.Name);
    }

    [TestMethod]
    public void AddPersistsGroupRetrievableFromNewContext()
    {
        _repository.Add(new Group("B"));                // autosave por operacion

        Group? stored = _repository.GetByName("B");     // contexto nuevo

        Assert.IsNotNull(stored);
        Assert.AreEqual("B", stored!.Name);
    }

    [TestMethod]
    public void UpdateAddsGroupWhenNameDoesNotExist()
    {
        _repository.Update(new Group("C"));

        Assert.IsTrue(_repository.ExistsByName("C"));
    }

    [TestMethod]
    public void DeleteRemovesStoredGroup()
    {
        _repository.Add(new Group("D"));

        _repository.Delete(new Group("D"));

        Assert.IsNull(_repository.GetByName("D"));
    }

    [TestMethod]
    public void ExistsByNameReturnsTrueWhenGroupIsStored()
    {
        _repository.Add(new Group("F"));

        Assert.IsTrue(_repository.ExistsByName("F"));
    }

    [TestMethod]
    public void ExistsByNameReturnsFalseWhenGroupIsNotStored()
    {
        Assert.IsFalse(_repository.ExistsByName("Z"));
    }

    [TestMethod]
    public void GetAllReturnsAllStoredGroups()
    {
        _repository.Add(new Group("G"));
        _repository.Add(new Group("H"));

        IReadOnlyCollection<Group> groups = _repository.GetAll();

        Assert.AreEqual(2, groups.Count);
        Assert.IsTrue(groups.Any(group => group.Name == "G"));
        Assert.IsTrue(groups.Any(group => group.Name == "H"));
    }

    [TestMethod]
    public void ClearRemovesAllStoredGroups()
    {
        _repository.Add(new Group("A"));
        _repository.Add(new Group("B"));

        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void AddPersistsGroupWithItsFourTeamsAndGetByNameReturnsThem()
    {
        Team[] teams = SeedFourTeams(baseId: 10);
        Group group = EfFixtureData.MakeGroup("A", teams);

        _repository.Add(group);                         // los equipos ya existen: se attachean, no se duplican

        Group? stored = _repository.GetByName("A");
        Assert.IsNotNull(stored);
        Assert.AreEqual(4, stored!.Teams.Count);
        Assert.IsTrue(stored.Teams.Any(team => team.Id == 10));
        Assert.IsTrue(stored.Teams.Any(team => team.Id == 13));
    }

    [TestMethod]
    public void AddGroupWithExistingTeamsDoesNotDuplicateTeamRows()
    {
        Team[] teams = SeedFourTeams(baseId: 20);

        _repository.Add(EfFixtureData.MakeGroup("A", teams));

        Assert.AreEqual(4, EfFixtureData.Count<Team>(_factory));   // sin filas de equipo duplicadas
        Assert.AreEqual(1, EfFixtureData.Count<Group>(_factory));
    }

    [TestMethod]
    public void ClearReleasesTeamsInsteadOfDeletingThem()
    {
        Team[] teams = SeedFourTeams(baseId: 30);
        _repository.Add(EfFixtureData.MakeGroup("A", teams));

        _repository.Clear();

        Assert.AreEqual(0, EfFixtureData.Count<Group>(_factory));
        Assert.AreEqual(4, EfFixtureData.Count<Team>(_factory));   // SetNull: los equipos sobreviven al borrado del grupo
    }

    // Persiste 4 equipos sueltos (sin grupo) en la BD compartida y devuelve esas instancias.
    private Team[] SeedFourTeams(int baseId)
    {
        Team[] teams =
        [
            EfFixtureData.MakeTeam(baseId, $"Team-{baseId}"),
            EfFixtureData.MakeTeam(baseId + 1, $"Team-{baseId + 1}"),
            EfFixtureData.MakeTeam(baseId + 2, $"Team-{baseId + 2}"),
            EfFixtureData.MakeTeam(baseId + 3, $"Team-{baseId + 3}"),
        ];
        EfFixtureData.Seed(_factory, teams);
        return teams;
    }
}
