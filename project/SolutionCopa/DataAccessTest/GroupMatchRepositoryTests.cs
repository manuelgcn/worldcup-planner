using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest
{
    [TestClass]
    public class GroupMatchRepositoryTests
    {
        [TestMethod]
        public void AddAndGetByIdAndGroup()
        {
            var repo = new GroupMatchRepository();

            var team1 = new Team(1, "TeamA", 1000, false, Confederation.UEFA);
            var team2 = new Team(2, "TeamB", 900, false, Confederation.UEFA);
            var stadium = new Stadium("S1", "City", null, 30000);
            var group = new Group("A");
            var journey = Journey.CreateInstance(1);

            var match = MatchGroup.CreateInstance(1, DateTime.Now.AddDays(1), team1, team2, stadium, group, journey);

            repo.Add(match);

            var fetched = repo.GetById(1);
            Assert.IsNotNull(fetched);

            var byGroup = repo.GetByGroup("A");
            Assert.IsTrue(byGroup.Count > 0);
        }
    }
}