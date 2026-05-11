using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest
{
    [TestClass]
    public class EliminationMatchRepositoryTests
    {
        [TestMethod]
        public void AddAndGetByPhase()
        {
            var repo = new EliminationMatchRepository();

            var team1 = new Team(1, "TeamA", 1200, false, Confederation.UEFA);
            var team2 = new Team(2, "TeamB", 1100, false, Confederation.UEFA);
            var stadium = new Stadium("S1", "City", null, 30000);

            var source1 = MatchGroup.CreateInstance(10, DateTime.Now.AddDays(-1), team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
            var source2 = MatchGroup.CreateInstance(11, DateTime.Now.AddDays(-1), team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

            var elim = MatchElimination.Create(20, DateTime.Now.AddDays(1), team1, team2, stadium, Phase.RoundOf16, "R16-1", source1, source2);

            repo.Add(elim);

            var fetched = repo.GetById(20);
            Assert.IsNotNull(fetched);

            var byPhase = repo.GetByPhase(Phase.RoundOf16);
            Assert.IsTrue(byPhase.Count > 0);
        }
    }
}