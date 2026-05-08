using ProjectCopa;
using Services.Interfaces;

namespace Services;

public class MatchSimulationEngine : IMatchSimulationEngine
{
    private const int MaxGoals = 15;
    private const double BaseScoringChance = 0.25;
    private const double RankingWeightFactor = 0.0002;
    private const double MinimumScoringChance = 0.10;
    private const double MaximumScoringChance = 0.80;
    private const string HomeTeamRequiredMessage = "Home team is required for simulation.";
    private const string AwayTeamRequiredMessage = "Away team is required for simulation.";

    public void Simulate(Match match, int simulationSeed)
    {
        ArgumentNullException.ThrowIfNull(match);

        Random random = new Random(simulationSeed);

        Team homeTeam = match.HomeTeam ?? throw new InvalidOperationException(HomeTeamRequiredMessage);
        Team awayTeam = match.AwayTeam ?? throw new InvalidOperationException(AwayTeamRequiredMessage);

        int homeGoals = GenerateGoals(random, homeTeam.RankingFifa, awayTeam.RankingFifa);
        int awayGoals = GenerateGoals(random, awayTeam.RankingFifa, homeTeam.RankingFifa);

        match.SetGoals(homeGoals, awayGoals);
    }

    private int GenerateGoals(Random random, int teamRanking, int opponentRanking)
    {
        double scoringChance = CalculateScoringChance(teamRanking, opponentRanking);

        int goals = 0;
        double currentChance = scoringChance;

        while (random.NextDouble() < currentChance && goals < MaxGoals)
        {
            goals++;

            currentChance *= 0.75;

            if (currentChance < 0.01)
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
