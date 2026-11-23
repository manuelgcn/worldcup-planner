using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public sealed class EloForecastTests
{
    [TestMethod]
    public void ExpectedScore_EqualRankings_IsOneHalf()
    {
        double expected = EloRankingCalculator.ExpectedScore(1500, 1500);

        Assert.AreEqual(0.5, expected, 1e-9);
    }

    // Mismo ejemplo de la letra que usa Compute: B (1200) contra A (1500) espera 1/(1+10^0.3) = 0.33386.
    [TestMethod]
    public void ExpectedScore_WeakTeamAgainstStrongTeam_MatchesLetterExample()
    {
        double expected = EloRankingCalculator.ExpectedScore(1200, 1500);

        Assert.AreEqual(0.33386, expected, 1e-5);
    }

    [TestMethod]
    public void Forecast_GroupStageWithEqualRankings_SplitsEvenlyAroundTheLargestDrawShare()
    {
        EloForecast forecast = EloRankingCalculator.Forecast(1500, 1500, MatchPhase.GroupStage);

        Assert.AreEqual(new EloForecast(37, 26, 37), forecast);
    }

    // E = 0.66614: empate = 26 % * (1 - |2E - 1|) = 17,4 %; local = E - empate/2 = 57,9 %; visita = 24,7 %.
    [TestMethod]
    public void Forecast_GroupStageWithFavourite_ShrinksTheDrawAndFavoursTheStrongerTeam()
    {
        EloForecast forecast = EloRankingCalculator.Forecast(1500, 1200, MatchPhase.GroupStage);

        Assert.AreEqual(new EloForecast(58, 17, 25), forecast);
    }

    [TestMethod]
    public void Forecast_Knockout_HasNoDraw()
    {
        EloForecast forecast = EloRankingCalculator.Forecast(1500, 1200, MatchPhase.Knockout);

        Assert.AreEqual(new EloForecast(67, 0, 33), forecast);
    }

    [TestMethod]
    public void Forecast_AnyRankingPair_SumsToOneHundredWithoutNegativesAndMirrorsWhenSwapped()
    {
        foreach (MatchPhase phase in Enum.GetValues<MatchPhase>())
        {
            for (int home = 300; home <= 2500; home += 50)
            {
                for (int away = 300; away <= 2500; away += 50)
                {
                    EloForecast forecast = EloRankingCalculator.Forecast(home, away, phase);
                    EloForecast swapped = EloRankingCalculator.Forecast(away, home, phase);
                    string pair = $"{home} vs {away} ({phase})";

                    Assert.AreEqual(100, forecast.HomeWin + forecast.Draw + forecast.AwayWin, pair);
                    Assert.IsTrue(forecast.HomeWin >= 0 && forecast.Draw >= 0 && forecast.AwayWin >= 0, pair);
                    Assert.AreEqual(forecast.HomeWin, swapped.AwayWin, pair);
                    Assert.AreEqual(forecast.Draw, swapped.Draw, pair);
                }
            }
        }
    }
}
