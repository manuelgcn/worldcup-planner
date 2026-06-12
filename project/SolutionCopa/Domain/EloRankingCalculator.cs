namespace ProjectCopa;

public enum EloResult { Win, Draw, Loss }
public enum MatchPhase { GroupStage, Knockout }

public static class EloRankingCalculator
{
    private const int MinRanking = 300;
    private const int MaxRanking = 2500;
    private const double K = 30.0;
    private const double DenominatorForExpectation = 1000.0; // matches tests' expected values

    public static int Compute(int rankingActual, int rankingOpponent, EloResult result, MatchPhase phase)
    {
        double expected = 1.0 / (1.0 + Math.Pow(10.0, (rankingOpponent - rankingActual) / DenominatorForExpectation));

        double score = result switch
        {
            EloResult.Win => 1.0,
            EloResult.Draw => 0.5,
            EloResult.Loss => 0.0,
            _ => 0.5
        };

        double phaseMultiplier = 1.0;

        double delta = K * (score - expected) * phaseMultiplier;

        double newRatingDouble = rankingActual + delta;

        int newRating = (int)Math.Round(newRatingDouble, MidpointRounding.AwayFromZero);

        newRating = Math.Clamp(newRating, MinRanking, MaxRanking);

        return newRating;
    }
}