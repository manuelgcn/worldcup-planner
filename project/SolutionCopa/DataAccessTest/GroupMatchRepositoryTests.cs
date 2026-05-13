using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest;

[TestClass]
public class GroupMatchRepositoryTests
{
    [TestMethod]
    public void AddAndGetByIdAndGroup()
    {
        GroupMatchRepository repo = new GroupMatchRepository();

        Team team1 = new Team(1, "TeamA", 1000, false, Confederation.UEFA);
        Team team2 = new Team(2, "TeamB", 900, false, Confederation.UEFA);
        Stadium stadium = new Stadium("S1", "City", null, 30000);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);

        MatchGroup match = MatchGroup.CreateInstance(1, DateTime.Now.AddDays(1), team1, team2, stadium, group, journey);

        repo.Add(match);

        Match? fetched = repo.GetById(1);
        Assert.IsNotNull(fetched);

        IReadOnlyCollection<Match> byGroup = repo.GetByGroup("A");
        Assert.IsTrue(byGroup.Count > 0);
    }
}