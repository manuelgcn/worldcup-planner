using ProjectCopa;
using Services.Interfaces;

namespace Services;

public sealed class ProbabilisticSimulationEngine : IMatchSimulationEngine
{
    public const string EngineName = "Probabilístico";

    private const int MaxGoals = 15;
    private const double GoalDecayFactor = 0.65;
    private const double BaseScoringChance = 0.55;
    private const double RankingWeightFactor = 0.0005;
    private const double MinimumScoringChance = 0.10;
    private const double MaximumScoringChance = 0.85;
    private const int PerMatchSeedMultiplier = 397;
    private const int PenaltyKicksPerTeam = 5;
    private const int SuddenDeathSafetyCap = 50;
    private const double BasePenaltyConversionChance = 0.75;
    private const double PenaltyRankingWeightFactor = 0.00015;
    private const double MinimumPenaltyConversionChance = 0.40;
    private const double MaximumPenaltyConversionChance = 0.95;
    private const string HomeTeamRequiredMessage = "Home team is required for simulation.";
    private const string AwayTeamRequiredMessage = "Away team is required for simulation.";

    public string Name => EngineName;

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

        if (match is MatchElimination eliminationMatch && eliminationMatch.IsDraw())
        {
            ResolveByPenaltyShootout(eliminationMatch, random, homeTeam.FifaRanking, awayTeam.FifaRanking);
        }
    }

    private static void ResolveByPenaltyShootout(MatchElimination match, Random random, int homeRanking, int awayRanking)
    {
        double homeChance = CalculatePenaltyConversionChance(homeRanking, awayRanking);
        double awayChance = CalculatePenaltyConversionChance(awayRanking, homeRanking);

        int homePenalties = 0;
        int awayPenalties = 0;

        for (int kick = 0; kick < PenaltyKicksPerTeam; kick++)
        {
            if (random.NextDouble() < homeChance)
            {
                homePenalties++;
            }
            if (random.NextDouble() < awayChance)
            {
                awayPenalties++;
            }
        }

        int suddenDeathKick = 0;
        while (homePenalties == awayPenalties && suddenDeathKick < SuddenDeathSafetyCap)
        {
            bool homeScored = random.NextDouble() < homeChance;
            bool awayScored = random.NextDouble() < awayChance;
            if (homeScored)
            {
                homePenalties++;
            }
            if (awayScored)
            {
                awayPenalties++;
            }
            suddenDeathKick++;
        }

        if (homePenalties == awayPenalties)
        {
            homePenalties++;
        }

        match.SetPenaltyShootoutResult(homePenalties, awayPenalties);
    }

    private static double CalculatePenaltyConversionChance(int shooterRanking, int opponentRanking)
    {
        int rankingDifference = shooterRanking - opponentRanking;
        double adjustedChance = BasePenaltyConversionChance + (rankingDifference * PenaltyRankingWeightFactor);

        if (adjustedChance < MinimumPenaltyConversionChance)
        {
            return MinimumPenaltyConversionChance;
        }

        if (adjustedChance > MaximumPenaltyConversionChance)
        {
            return MaximumPenaltyConversionChance;
        }

        return adjustedChance;
    }

    // Publico para que otros motores puedan derivar la misma semilla estable por partido.
    public static int DerivePerMatchSeed(int simulationSeed, int matchId)
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
