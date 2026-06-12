using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DomainTest;

[TestClass]
public sealed class EloRankingCycleTests
{
    [TestMethod]
    public void Compute_WeakTeamWinsInGroupStage_ShouldReturn1220()
    {
        // 1200 vs 1500, Win, GroupStage
        int result = EloRankingCalculator.Compute(1200, 1500, EloResult.Win, MatchPhase.GroupStage);
        Assert.AreEqual(1220, result);
    }
}
