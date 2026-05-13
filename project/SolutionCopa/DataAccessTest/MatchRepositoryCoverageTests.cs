using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest;

[TestClass]
public class MatchRepositoryCoverageTests
{
    [TestMethod]
    public void Add_Update_Delete_And_Query_MatchRepository()
    {
        MatchRepository repo = new MatchRepository();

        Team team1 = new Team(1, "T1", 1200, false, Confederation.UEFA);
        Team team2 = new Team(2, "T2", 1100, false, Confederation.UEFA);
        Stadium stadium = new Stadium("S1", "City", null, 30000);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);

        MatchGroup match = MatchGroup.CreateInstance(1, DateTime.UtcNow.AddDays(1), team1, team2, stadium, group, journey);

        // Add
        repo.Add(match);
        Assert.IsTrue(repo.ExistsById(1));
        Match? fetched = repo.GetById(1);
        Assert.IsNotNull(fetched);

        // Query by group and journey
        List<Match> byGroup = repo.GetByGroup("A");
        Assert.IsTrue(byGroup.Any(m => m.MatchId == 1));

        List<Match> byJourney = repo.GetByJourney(1);
        Assert.IsTrue(byJourney.Any(m => m.MatchId == 1));

        // Date range
        DateTime from = DateTime.UtcNow;
        DateTime to = DateTime.UtcNow.AddDays(7);
        List<Match> byDate = repo.GetByDateRange(from, to);
        Assert.IsTrue(byDate.Any(m => m.MatchId == 1));

        // Update (replace)
        MatchGroup updated = MatchGroup.CreateInstance(1, DateTime.UtcNow.AddDays(2), team1, team2, stadium, group, journey);
        updated.SetGoals(2, 1);
        repo.Update(updated);

        Match? fetchedUpdated = repo.GetById(1);
        Assert.IsNotNull(fetchedUpdated);
        Assert.AreEqual(2, fetchedUpdated!.HomeGoals);

        // Delete
        repo.Delete(updated);
        Assert.IsFalse(repo.ExistsById(1));
    }
}