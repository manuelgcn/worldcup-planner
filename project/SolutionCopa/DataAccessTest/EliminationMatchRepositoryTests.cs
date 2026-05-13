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
            EliminationMatchRepository repo = new EliminationMatchRepository();

            Team team1 = new Team(1, "TeamA", 1200, false, Confederation.UEFA);
            Team team2 = new Team(2, "TeamB", 1100, false, Confederation.UEFA);
            Stadium stadium = new Stadium("S1", "City", null, 30000);

            MatchGroup source1 = MatchGroup.CreateInstance(10, DateTime.Now.AddDays(-1), team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
            MatchGroup source2 = MatchGroup.CreateInstance(11, DateTime.Now.AddDays(-1), team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

            MatchElimination elim = MatchElimination.Create(20, DateTime.Now.AddDays(1), team1, team2, stadium, Phase.RoundOf16, "R16-1", source1, source2);

            repo.Add(elim);

            Match? fetched = repo.GetById(20);
            Assert.IsNotNull(fetched);

            IReadOnlyCollection<Match> byPhase = repo.GetByPhase(Phase.RoundOf16);
            Assert.IsTrue(byPhase.Count > 0);
        }
    }
}