using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Motor "Aleatorio Puro": ignora los rankings FIFA y decide los goles solo con la semilla.
public sealed class PureRandomSimulationEngine : IMatchSimulationEngine
{
    public const string EngineName = "Aleatorio Puro";

    private const int MaxGoalsExclusive = 6;
    private const int PenaltyKicksPerTeam = 5;
    private const int SuddenDeathSafetyCap = 50;
    private const double PenaltyConversionChance = 0.5;

    public string Name => EngineName;

    public void Simulate(Match match, int simulationSeed)
    {
        ArgumentNullException.ThrowIfNull(match);

        Random random = new Random(ProbabilisticSimulationEngine.DerivePerMatchSeed(simulationSeed, match.MatchId));

        int homeGoals = random.Next(0, MaxGoalsExclusive);
        int awayGoals = random.Next(0, MaxGoalsExclusive);

        match.SetGoals(homeGoals, awayGoals);
        match.MarkAsSimulated();

        if (match is MatchElimination eliminationMatch && eliminationMatch.IsDraw())
        {
            ResolveByCoinFlipPenalties(eliminationMatch, random);
        }
    }

    private static void ResolveByCoinFlipPenalties(MatchElimination match, Random random)
    {
        int homePenalties = 0;
        int awayPenalties = 0;

        for (int kick = 0; kick < PenaltyKicksPerTeam; kick++)
        {
            if (random.NextDouble() < PenaltyConversionChance)
            {
                homePenalties++;
            }
            if (random.NextDouble() < PenaltyConversionChance)
            {
                awayPenalties++;
            }
        }

        int suddenDeathKick = 0;
        while (homePenalties == awayPenalties && suddenDeathKick < SuddenDeathSafetyCap)
        {
            if (random.NextDouble() < PenaltyConversionChance)
            {
                homePenalties++;
            }
            if (random.NextDouble() < PenaltyConversionChance)
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
}
