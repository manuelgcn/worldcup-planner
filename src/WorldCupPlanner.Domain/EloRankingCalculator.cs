namespace WorldCupPlanner.Domain;

public enum EloResult { Win, Draw, Loss }
public enum MatchPhase { GroupStage, Knockout }

// Porcentajes enteros que suman 100; Draw vale 0 en eliminatorias.
public readonly record struct EloForecast(int HomeWin, int Draw, int AwayWin);

public static class EloRankingCalculator
{
    private const int MinRanking = 300;
    private const int MaxRanking = 2500;
    private const double K = 30.0;
    private const double DenominatorForExpectation = 1000.0; // matches tests' expected values
    private const double MaxDrawShare = 0.26;
    private const int PercentTotal = 100;

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

        double phaseMultiplier = phase == MatchPhase.Knockout ? 1.5 : 1.0;

        double delta = K * (score - expected) * phaseMultiplier;

        double newRatingDouble = rankingActual + delta;

        int newRating = (int)Math.Round(newRatingDouble, MidpointRounding.AwayFromZero);

        newRating = Math.Clamp(newRating, MinRanking, MaxRanking);

        return newRating;
    }

    public static double ExpectedScore(int rankingActual, int rankingOpponent)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (rankingOpponent - rankingActual) / DenominatorForExpectation));
    }

    // Estimación para mostrar al lado de un partido pendiente: los motores de simulación no la usan.
    // El empate es MaxDrawShare con rankings iguales y se achica a medida que crece la diferencia;
    // local y visita se reparten el resto de modo que victoria + empate/2 siga siendo el puntaje esperado.
    public static EloForecast Forecast(int rankingHome, int rankingAway, MatchPhase phase)
    {
        double expected = ExpectedScore(rankingHome, rankingAway);

        if (phase == MatchPhase.Knockout)
        {
            int homeAdvances = ToPercent(expected);
            return new EloForecast(homeAdvances, 0, PercentTotal - homeAdvances);
        }

        double draw = MaxDrawShare * (1.0 - Math.Abs(2.0 * expected - 1.0));
        int homeWin = ToPercent(expected - draw / 2.0);
        int awayWin = ToPercent(1.0 - expected - draw / 2.0);

        // El empate absorbe el redondeo: así los tres suman 100 y el pronóstico es el mismo visto desde cualquiera de los dos equipos.
        return new EloForecast(homeWin, PercentTotal - homeWin - awayWin, awayWin);
    }

    private static int ToPercent(double share)
    {
        return (int)Math.Round(share * PercentTotal, MidpointRounding.AwayFromZero);
    }
}