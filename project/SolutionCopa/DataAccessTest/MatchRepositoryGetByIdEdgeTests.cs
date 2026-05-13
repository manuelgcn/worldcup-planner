using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest;

[TestClass]
public class MatchRepositoryGetByIdEdgeTests
{
    [TestMethod]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        MatchRepository repo = new MatchRepository();
        Match? result = repo.GetById(12345);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_ReturnsCorrectMatch_WhenMultipleExist()
    {
        MatchRepository repo = new MatchRepository();
        Team team1 = new Team(1, "A", 1000, false, Confederation.UEFA);
        Team team2 = new Team(2, "B", 1000, false, Confederation.UEFA);
        Stadium stadium = new Stadium("S", "C", null, 30000);
        MatchGroup m1 = MatchGroup.CreateInstance(10, DateTime.UtcNow, team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
        MatchGroup m2 = MatchGroup.CreateInstance(20, DateTime.UtcNow, team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

        repo.Add(m1);
        repo.Add(m2);

        Match? r1 = repo.GetById(10);
        Match? r2 = repo.GetById(20);

        Assert.IsNotNull(r1);
        Assert.AreEqual(10, r1!.MatchId);
        Assert.IsNotNull(r2);
        Assert.AreEqual(20, r2!.MatchId);
    }
}