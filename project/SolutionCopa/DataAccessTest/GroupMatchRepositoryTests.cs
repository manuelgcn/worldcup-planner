using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class GroupMatchRepositoryTests
{
    [TestMethod]
    public void AddAndGetByIdAndGroup()
    {
        InMemoryDbContextFactory factory = new InMemoryDbContextFactory();
        GroupMatchRepository repo = new GroupMatchRepository(factory);

        Team team1 = EfFixtureData.MakeTeam(1, "TeamA");
        Team team2 = EfFixtureData.MakeTeam(2, "TeamB");
        Stadium stadium = EfFixtureData.MakeStadium(1, "S1");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        EfFixtureData.Seed(factory, team1, team2, stadium, group, journey);

        repo.Add(EfFixtureData.MakeGroupMatch(1, new DateTime(2026, 6, 2), team1, team2, stadium, group, journey));

        Assert.IsNotNull(repo.GetById(1));
        Assert.IsTrue(repo.GetByGroup("A").Count > 0);
    }
}
