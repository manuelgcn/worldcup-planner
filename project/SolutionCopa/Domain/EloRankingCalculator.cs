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
        return 0;
    }
}