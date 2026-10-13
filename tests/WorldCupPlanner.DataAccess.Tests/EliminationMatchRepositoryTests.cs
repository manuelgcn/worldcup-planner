using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class EliminationMatchRepositoryTests
{
    private InMemoryDbContextFactory _factory = null!;
    private EliminationMatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new InMemoryDbContextFactory();
        _repository = new EliminationMatchRepository(_factory);
    }

    [TestMethod]
    public void AddAndGetByPhase()
    {
        _repository.Add(SeedSourcesAndBuildElimination(20, Phase.RoundOf16, sourceIdBase: 100, "X", "Y"));

        Assert.IsNotNull(_repository.GetById(20));
        Assert.IsTrue(_repository.GetByPhase(Phase.RoundOf16).Count > 0);
    }

    [TestMethod]
    public void ClearRemovesAllStoredEliminationMatches()
    {
        _repository.Add(SeedSourcesAndBuildElimination(20, Phase.RoundOf16, sourceIdBase: 100, "X", "Y"));
        _repository.Add(SeedSourcesAndBuildElimination(21, Phase.RoundOf16, sourceIdBase: 300, "P", "Q"));

        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    // Siembra dos partidos de grupo (fuentes) y devuelve un partido de eliminacion listo para Add.
    private MatchElimination SeedSourcesAndBuildElimination(int eliminationId, Phase phase, int sourceIdBase, string groupHome, string groupAway)
    {
        Team a = EfFixtureData.MakeTeam(sourceIdBase + 1, $"A{sourceIdBase}");
        Team b = EfFixtureData.MakeTeam(sourceIdBase + 2, $"B{sourceIdBase}");
        Team c = EfFixtureData.MakeTeam(sourceIdBase + 3, $"C{sourceIdBase}");
        Team d = EfFixtureData.MakeTeam(sourceIdBase + 4, $"D{sourceIdBase}");
        Stadium stadium = EfFixtureData.MakeStadium(sourceIdBase + 5, $"St{sourceIdBase}");
        Group groupX = new Group(groupHome);
        Group groupY = new Group(groupAway);
        Journey journey = Journey.CreateInstance(sourceIdBase);
        MatchGroup sourceHome = EfFixtureData.MakeGroupMatch(sourceIdBase + 10, new DateTime(2026, 6, 1), a, b, stadium, groupX, journey);
        MatchGroup sourceAway = EfFixtureData.MakeGroupMatch(sourceIdBase + 11, new DateTime(2026, 6, 1), c, d, stadium, groupY, journey);
        EfFixtureData.Seed(_factory, a, b, c, d, stadium, groupX, groupY, journey, sourceHome, sourceAway);

        return EfFixtureData.MakeEliminationMatch(eliminationId, new DateTime(2026, 6, 15), a, d, stadium, phase, sourceHome, sourceAway);
    }
}
