using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public sealed class EloRankingCalculatorTests
{
    // Ejemplo textual de la letra: A (1500) vs B (1200) en grupos y gana B.
    // Probabilidad de B = 1/(1+10^0.3) = 0.33386, delta = 30 * (1 - 0.33386) = 19.984,
    // 1200 + 19.984 = 1219.984 -> 1220.
    [TestMethod]
    public void Compute_WeakTeamWinsInGroupStage_MatchesLetterExample()
    {
        int result = EloRankingCalculator.Compute(1200, 1500, EloResult.Win, MatchPhase.GroupStage);

        Assert.AreEqual(1220, result);
    }

    // Lado simetrico: el favorito que pierde cede la misma cantidad de puntos.
    [TestMethod]
    public void Compute_StrongTeamLosesInGroupStage_DropsSymmetricAmount()
    {
        int result = EloRankingCalculator.Compute(1500, 1200, EloResult.Loss, MatchPhase.GroupStage);

        Assert.AreEqual(1480, result);
    }

    [TestMethod]
    public void Compute_WeakTeamDrawsInGroupStage_GainsHalfDelta()
    {
        int result = EloRankingCalculator.Compute(1200, 1500, EloResult.Draw, MatchPhase.GroupStage);

        Assert.AreEqual(1205, result);
    }

    [TestMethod]
    public void Compute_StrongTeamDrawsInGroupStage_LosesHalfDelta()
    {
        int result = EloRankingCalculator.Compute(1500, 1200, EloResult.Draw, MatchPhase.GroupStage);

        Assert.AreEqual(1495, result);
    }

    // Empate exacto en la ecuacion: delta = 30 * 0.5 * 1.5 = 22.5 -> redondea hacia arriba (1022.5 -> 1023).
    [TestMethod]
    public void Compute_ExactHalfDelta_RoundsAwayFromZeroUpwards()
    {
        int result = EloRankingCalculator.Compute(1000, 1000, EloResult.Win, MatchPhase.Knockout);

        Assert.AreEqual(1023, result);
    }

    // Mismo caso de la letra pero en eliminatorias: multiplicador 1.5 -> delta ~= 29.98 -> 1230.
    [TestMethod]
    public void Compute_KnockoutMultiplier_AppliesOnePointFiveFactor()
    {
        int result = EloRankingCalculator.Compute(1200, 1500, EloResult.Win, MatchPhase.Knockout);

        Assert.AreEqual(1230, result);
    }

    [TestMethod]
    public void Compute_ResultAboveUpperBound_ClampsTo2500()
    {
        int result = EloRankingCalculator.Compute(2495, 2495, EloResult.Win, MatchPhase.Knockout);

        Assert.AreEqual(2500, result);
    }

    [TestMethod]
    public void Compute_ResultBelowLowerBound_ClampsTo300()
    {
        int result = EloRankingCalculator.Compute(300, 2500, EloResult.Loss, MatchPhase.Knockout);

        Assert.AreEqual(300, result);
    }
}
