using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public sealed class QualifiedTeamsTests
{
    [TestMethod]
    public void QualifiedTeamsStartsWithEmptyCollections()
    {
        QualifiedTeams qualifiedTeams = new QualifiedTeams();

        Assert.IsNotNull(qualifiedTeams.TopEightFirsts);
        Assert.IsNotNull(qualifiedTeams.RemainingFourFirsts);
        Assert.IsNotNull(qualifiedTeams.TopEightSeconds);
        Assert.IsNotNull(qualifiedTeams.WorstFourSeconds);
        Assert.IsNotNull(qualifiedTeams.BestEightThirds);
        Assert.IsNotNull(qualifiedTeams.GroupByTeamId);
        Assert.AreEqual(0, qualifiedTeams.TopEightFirsts.Count);
        Assert.AreEqual(0, qualifiedTeams.RemainingFourFirsts.Count);
        Assert.AreEqual(0, qualifiedTeams.TopEightSeconds.Count);
        Assert.AreEqual(0, qualifiedTeams.WorstFourSeconds.Count);
        Assert.AreEqual(0, qualifiedTeams.BestEightThirds.Count);
        Assert.AreEqual(0, qualifiedTeams.GroupByTeamId.Count);
    }

    [TestMethod]
    public void QualifiedTeamsAllowsSettingCollections()
    {
        Team team = new Team(1, "Team", 1500, false, Confederation.CONMEBOL);
        List<Team> teams = new List<Team> { team };
        Dictionary<int, string> groupByTeamId = new Dictionary<int, string> { { team.Id, "A" } };
        QualifiedTeams qualifiedTeams = new QualifiedTeams
        {
            TopEightFirsts = teams,
            RemainingFourFirsts = teams,
            TopEightSeconds = teams,
            WorstFourSeconds = teams,
            BestEightThirds = teams,
            GroupByTeamId = groupByTeamId
        };

        Assert.AreSame(teams, qualifiedTeams.TopEightFirsts);
        Assert.AreSame(teams, qualifiedTeams.RemainingFourFirsts);
        Assert.AreSame(teams, qualifiedTeams.TopEightSeconds);
        Assert.AreSame(teams, qualifiedTeams.WorstFourSeconds);
        Assert.AreSame(teams, qualifiedTeams.BestEightThirds);
        Assert.AreSame(groupByTeamId, qualifiedTeams.GroupByTeamId);
    }

    [TestMethod]
    public void QualifiedTeamsRecordSupportsValueEquality()
    {
        Team team = new Team(1, "Team", 1500, false, Confederation.CONMEBOL);
        List<Team> teams = new List<Team> { team };
        Dictionary<int, string> groupByTeamId = new Dictionary<int, string> { { team.Id, "A" } };
        QualifiedTeams first = new QualifiedTeams
        {
            TopEightFirsts = teams,
            RemainingFourFirsts = teams,
            TopEightSeconds = teams,
            WorstFourSeconds = teams,
            BestEightThirds = teams,
            GroupByTeamId = groupByTeamId
        };
        QualifiedTeams second = new QualifiedTeams
        {
            TopEightFirsts = teams,
            RemainingFourFirsts = teams,
            TopEightSeconds = teams,
            WorstFourSeconds = teams,
            BestEightThirds = teams,
            GroupByTeamId = groupByTeamId
        };

        Assert.IsTrue(first.Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.IsTrue(first.ToString()!.Contains(nameof(QualifiedTeams)));
    }
}
