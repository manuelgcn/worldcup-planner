using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest
{
    [TestClass]
    public class MatchRepositoryCoverageTests
    {
        [TestMethod]
        public void Add_Update_Delete_And_Query_MatchRepository()
        {
            var repo = new MatchRepository();

            var team1 = new Team(1, "T1", 1200, false, Confederation.UEFA);
            var team2 = new Team(2, "T2", 1100, false, Confederation.UEFA);
            var stadium = new Stadium("S1", "City", null, 30000);
            var group = new Group("A");
            var journey = Journey.CreateInstance(1);

            var match = MatchGroup.CreateInstance(1, DateTime.UtcNow.AddDays(1), team1, team2, stadium, group, journey);

            // Add
            repo.Add(match);
            Assert.IsTrue(repo.ExistsById(1));
            var fetched = repo.GetById(1);
            Assert.IsNotNull(fetched);

            // Query by group and journey
            var byGroup = repo.GetByGroup("A");
            Assert.IsTrue(byGroup.Any(m => m.MatchId == 1));

            var byJourney = repo.GetByJourney(1);
            Assert.IsTrue(byJourney.Any(m => m.MatchId == 1));

            // Date range
            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(7);
            var byDate = repo.GetByDateRange(from, to);
            Assert.IsTrue(byDate.Any(m => m.MatchId == 1));

            // Update (replace)
            var updated = MatchGroup.CreateInstance(1, DateTime.UtcNow.AddDays(2), team1, team2, stadium, group, journey);
            updated.SetGoals(2, 1);
            repo.Update(updated);

            var fetchedUpdated = repo.GetById(1);
            Assert.IsNotNull(fetchedUpdated);
            Assert.AreEqual(2, fetchedUpdated!.HomeGoals);

            // Delete
            repo.Delete(updated);
            Assert.IsFalse(repo.ExistsById(1));
        }
    }
}
