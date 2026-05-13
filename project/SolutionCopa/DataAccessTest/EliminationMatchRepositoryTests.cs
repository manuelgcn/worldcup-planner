using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;
using DataAccess.Repositories;

namespace DataAccessTest;

[TestClass]
public class EliminationMatchRepositoryTests
{
    [TestMethod]
    public void AddAndGetByPhase()
    {
        EliminationMatchRepository repo = new EliminationMatchRepository();

        MatchElimination elim = CreateEliminationMatch(20);

        repo.Add(elim);

        Match? fetched = repo.GetById(20);
        Assert.IsNotNull(fetched);

        IReadOnlyCollection<Match> byPhase = repo.GetByPhase(Phase.RoundOf16);
        Assert.IsTrue(byPhase.Count > 0);
    }

    [TestMethod]
    public void ClearRemovesAllStoredEliminationMatches()
    {
        EliminationMatchRepository repo = new EliminationMatchRepository();
        repo.Add(CreateEliminationMatch(20));
        repo.Add(CreateEliminationMatch(21));

        repo.Clear();

        Assert.AreEqual(0, repo.GetAll().Count);
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        EliminationMatchRepository repo = new EliminationMatchRepository();

        repo.Clear();

        Assert.AreEqual(0, repo.GetAll().Count);
    }

    private static MatchElimination CreateEliminationMatch(int id)
    {
        Team team1 = new Team(1, "TeamA", 1200, false, Confederation.UEFA);
        Team team2 = new Team(2, "TeamB", 1100, false, Confederation.UEFA);
        Stadium stadium = new Stadium("S1", "City", null, 30000);

        MatchGroup source1 = MatchGroup.CreateInstance(id + 100, DateTime.Now.AddDays(-1), team1, team2, stadium, new Group("A"), Journey.CreateInstance(1));
        MatchGroup source2 = MatchGroup.CreateInstance(id + 200, DateTime.Now.AddDays(-1), team2, team1, stadium, new Group("B"), Journey.CreateInstance(1));

        return MatchElimination.Create(id, DateTime.Now.AddDays(1), team1, team2, stadium, Phase.RoundOf16, $"R16-{id}", source1, source2);
    }
}