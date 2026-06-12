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

    [TestMethod]
    public void Compute_ExactHalfDelta_ShouldRoundUpwards()
    {
        // 1000 vs 1000, Win, Knockout -> delta = 30 * (1 - 0.5) * 1.5 = 22.5
        // 1000 + 22.5 = 1022.5 -> Should round to 1023
        int result = EloRankingCalculator.Compute(1000, 1000, EloResult.Win, MatchPhase.Knockout);
        Assert.AreEqual(1023, result);
    }

    [TestMethod]
    public void Compute_ResultAboveUpperBound_ShouldClampTo2500()
    {
        // 2495 vs 2495, Win, Knockout -> delta = 22.5 -> 23
        // 2495 + 23 = 2518 -> Should clamp to 2500
        int result = EloRankingCalculator.Compute(2495, 2495, EloResult.Win, MatchPhase.Knockout);
        Assert.AreEqual(2500, result);
    }

    [TestMethod]
    public void Compute_ResultBelowLowerBound_ShouldClampTo300()
    {
        // 300 vs 2500, Loss, Knockout -> delta will be negative
        // Should clamp to 300
        int result = EloRankingCalculator.Compute(300, 2500, EloResult.Loss, MatchPhase.Knockout);
        Assert.AreEqual(300, result);
    }
}
