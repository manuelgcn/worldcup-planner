using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class MatchSimulationEngine : IMatchSimulationEngine
{
    private const int MaxGoals = 15;
    private const double GoalDecayFactor = 0.65;
    private const double BaseScoringChance = 0.55;
    private const double RankingWeightFactor = 0.0005;
    private const double MinimumScoringChance = 0.10;
    private const double MaximumScoringChance = 0.85;
    private const int PerMatchSeedMultiplier = 397;
    private const string HomeTeamRequiredMessage = "Home team is required for simulation.";
    private const string AwayTeamRequiredMessage = "Away team is required for simulation.";

    public void Simulate(Match match, int simulationSeed)
    {
        ArgumentNullException.ThrowIfNull(match);

        Team homeTeam = match.HomeTeam ?? throw new InvalidOperationException(HomeTeamRequiredMessage);
        Team awayTeam = match.AwayTeam ?? throw new InvalidOperationException(AwayTeamRequiredMessage);

        Random random = new Random(DerivePerMatchSeed(simulationSeed, match.MatchId));

        int homeGoals = GenerateGoals(random, homeTeam.FifaRanking, awayTeam.FifaRanking);
        int awayGoals = GenerateGoals(random, awayTeam.FifaRanking, homeTeam.FifaRanking);

        match.SetGoals(homeGoals, awayGoals);
        match.MarkAsSimulated();
    }

    private static int DerivePerMatchSeed(int simulationSeed, int matchId)
    {
        unchecked
        {
            return (simulationSeed * PerMatchSeedMultiplier) ^ matchId;
        }
    }

    private static int GenerateGoals(Random random, int teamRanking, int opponentRanking)
    {
        double scoringChance = CalculateScoringChance(teamRanking, opponentRanking);

        int goals = 0;
        double currentChance = scoringChance;

        while (random.NextDouble() < currentChance && goals < MaxGoals)
        {
            goals++;
            currentChance *= GoalDecayFactor;

            if (currentChance < MinimumScoringChance)
            {
                break;
            }
        }

        return goals;
    }

    private static double CalculateScoringChance(int teamRanking, int opponentRanking)
    {
        int rankingDifference = teamRanking - opponentRanking;
        double adjustedChance = BaseScoringChance + (rankingDifference * RankingWeightFactor);

        if (adjustedChance < MinimumScoringChance)
        {
            return MinimumScoringChance;
        }

        if (adjustedChance > MaximumScoringChance)
        {
            return MaximumScoringChance;
        }

        return adjustedChance;
    }
}
