using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class MatchRepositoryGetByIdEdgeTests
{
    [TestMethod]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        MatchRepository repo = new MatchRepository(new InMemoryDbContextFactory());

        Assert.IsNull(repo.GetById(12345));
    }

    [TestMethod]
    public void GetById_ReturnsCorrectMatch_WhenMultipleExist()
    {
        InMemoryDbContextFactory factory = new InMemoryDbContextFactory();
        MatchRepository repo = new MatchRepository(factory);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        Stadium stadium = EfFixtureData.MakeStadium(1, "S");
        Team first = EfFixtureData.MakeTeam(1, "A");
        Team second = EfFixtureData.MakeTeam(2, "B");
        EfFixtureData.Seed(factory, group, journey, stadium, first, second);

        repo.Add(EfFixtureData.MakeGroupMatch(10, new DateTime(2026, 6, 1), first, second, stadium, group, journey));
        repo.Add(EfFixtureData.MakeGroupMatch(20, new DateTime(2026, 6, 2), second, first, stadium, group, journey));

        Assert.AreEqual(10, repo.GetById(10)!.MatchId);
        Assert.AreEqual(20, repo.GetById(20)!.MatchId);
    }
}
