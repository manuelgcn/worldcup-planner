using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class MatchRepositoryCoverageTests
{
    [TestMethod]
    public void Add_Update_Delete_And_Query_MatchRepository()
    {
        InMemoryDbContextFactory factory = new InMemoryDbContextFactory();
        MatchRepository repo = new MatchRepository(factory);

        Team team1 = EfFixtureData.MakeTeam(1, "T1");
        Team team2 = EfFixtureData.MakeTeam(2, "T2");
        Stadium stadium = EfFixtureData.MakeStadium(1, "S1");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        EfFixtureData.Seed(factory, team1, team2, stadium, group, journey);

        DateTime date = new DateTime(2026, 6, 2);
        repo.Add(EfFixtureData.MakeGroupMatch(1, date, team1, team2, stadium, group, journey));

        Assert.IsTrue(repo.ExistsById(1));
        Assert.IsNotNull(repo.GetById(1));
        Assert.IsTrue(repo.GetByGroup("A").Any(match => match.MatchId == 1));
        Assert.IsTrue(repo.GetByJourney(1).Any(match => match.MatchId == 1));
        Assert.IsTrue(repo.GetByDateRange(new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)).Any(match => match.MatchId == 1));

        Match stored = repo.GetById(1)!;
        stored.SetGoals(2, 1);
        repo.Update(stored);
        Assert.AreEqual(2, repo.GetById(1)!.HomeGoals);

        repo.Delete(repo.GetById(1)!);
        Assert.IsFalse(repo.ExistsById(1));
    }
}
