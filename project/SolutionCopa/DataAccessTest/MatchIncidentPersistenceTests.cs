using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

// Integracion sobre SQLite (relacional: valida FK y la jerarquia TPH de incidencias),
// igual que el resto de tests de persistencia delicada.
[TestClass]
public sealed class MatchIncidentPersistenceTests
{
    private SqliteDbContextFactory _factory = null!;
    private MatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new SqliteDbContextFactory();
        _repository = new MatchRepository(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void AddPersistsIncidentsAndGetByIdReturnsThemByConcreteType()
    {
        Team home = EfFixtureData.MakeTeam(11, "Home");
        Team away = EfFixtureData.MakeTeam(12, "Away");
        Stadium stadium = EfFixtureData.MakeStadium(13, "Stadium");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        EfFixtureData.Seed(_factory, home, away, stadium, group, journey);

        MatchGroup match = EfFixtureData.MakeGroupMatch(1, new DateTime(2026, 6, 1), home, away, stadium, group, journey);
        match.AddIncident(new YellowCardIncident(1, 20, home));
        match.AddIncident(new RedCardIncident(2, 70, home));
        match.AddIncident(new YellowCardIncident(3, 35, away));
        _repository.Add(match);

        Match reloaded = _repository.GetById(1)!;

        Assert.AreEqual(3, reloaded.Incidents.Count);
        Assert.AreEqual(2, reloaded.Incidents.OfType<YellowCardIncident>().Count());
        Assert.AreEqual(1, reloaded.Incidents.OfType<RedCardIncident>().Count());
        Assert.AreEqual(1, reloaded.CountRedCards(reloaded.HomeTeam!));
        Assert.AreEqual(1, reloaded.CountYellowCards(reloaded.AwayTeam!));
    }

    [TestMethod]
    public void UpdatePersistsIncidentAdditionsAndRemovals()
    {
        Match persisted = SeedMatchWithIncidents();

        persisted.RemoveIncident(persisted.Incidents.OfType<RedCardIncident>().Single());
        persisted.AddIncident(new YellowCardIncident(3, 55, persisted.AwayTeam!));
        _repository.Update(persisted);

        Match reloaded = _repository.GetById(1)!;
        Assert.AreEqual(2, reloaded.Incidents.Count);
        Assert.AreEqual(0, reloaded.Incidents.OfType<RedCardIncident>().Count());
        Assert.AreEqual(1, reloaded.CountYellowCards(reloaded.HomeTeam!));
        Assert.AreEqual(1, reloaded.CountYellowCards(reloaded.AwayTeam!));
    }

    [TestMethod]
    public void UpdateKeepsUnchangedIncidents()
    {
        Match persisted = SeedMatchWithIncidents();

        persisted.SetGoals(2, 1);
        _repository.Update(persisted);

        Match reloaded = _repository.GetById(1)!;
        Assert.AreEqual(2, reloaded.Incidents.Count);
        Assert.AreEqual(2, reloaded.HomeGoals);
    }

    [TestMethod]
    public void NextIncidentIdReturnsOneWhenThereAreNoIncidents()
    {
        Assert.AreEqual(1, _repository.NextIncidentId());
    }

    [TestMethod]
    public void NextIncidentIdReturnsMaxPlusOne()
    {
        SeedMatchWithIncidents();

        Assert.AreEqual(3, _repository.NextIncidentId());
    }

    private Match SeedMatchWithIncidents()
    {
        Team home = EfFixtureData.MakeTeam(11, "Home");
        Team away = EfFixtureData.MakeTeam(12, "Away");
        Stadium stadium = EfFixtureData.MakeStadium(13, "Stadium");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        EfFixtureData.Seed(_factory, home, away, stadium, group, journey);

        MatchGroup match = EfFixtureData.MakeGroupMatch(1, new DateTime(2026, 6, 1), home, away, stadium, group, journey);
        match.AddIncident(new YellowCardIncident(1, 20, home));
        match.AddIncident(new RedCardIncident(2, 70, home));
        _repository.Add(match);

        return _repository.GetById(1)!;
    }
}
