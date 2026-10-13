using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public class GroupQualifierTests
{
    private const int Seed = 0;
    private const int GroupIndex = 0;

    [TestMethod]
    public void RankGroup_CountsOnlyMatchesWithAResult()
    {
        Team alpha = new Team(1, "Alpha", 1500, false, Confederation.UEFA);
        Team bravo = new Team(2, "Bravo", 1500, false, Confederation.CAF);
        Group group = new Group("A");
        group.AddTeam(alpha);
        group.AddTeam(bravo);
        Stadium stadium = new Stadium("Centenario", "Montevideo", null, 60000);
        MatchGroup played = MatchGroup.CreateInstance(1, new DateTime(2026, 6, 1), alpha, bravo, stadium, group, Journey.CreateInstance(1));
        played.SetGoals(2, 0);
        MatchGroup pending = MatchGroup.CreateInstance(2, new DateTime(2026, 6, 4), bravo, alpha, stadium, group, Journey.CreateInstance(2));

        List<GroupStanding> table = GroupQualifier.RankGroup(group, new List<Match> { played, pending }, Seed, GroupIndex);

        Assert.AreSame(alpha, table[0].Team);
        Assert.AreEqual(1, table[0].GamesPlayed);
        Assert.AreEqual(3, table[0].Points);
        Assert.AreEqual(1, table[1].GamesPlayed);
        Assert.AreEqual(1, table[1].Losses);
        Assert.AreEqual(-2, table[1].GoalDifference);
    }
}
