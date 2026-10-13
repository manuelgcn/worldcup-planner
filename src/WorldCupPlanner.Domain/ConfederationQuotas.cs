namespace WorldCupPlanner.Domain;

// Cupos por confederacion del Mundial de 48 equipos. Los usan el alta manual, la importacion
// CSV y el completado con equipos ficticios, asi que viven en un solo lugar.
public static class ConfederationQuotas
{
    private const int AfcQuota = 8;
    private const int CafQuota = 9;
    private const int ConcacafQuota = 7;
    private const int ConmebolQuota = 7;
    private const int OfcQuota = 1;
    private const int UefaQuota = 16;

    public static IReadOnlyDictionary<Confederation, int> ByConfederation { get; } =
        new Dictionary<Confederation, int>
        {
            { Confederation.AFC, AfcQuota },
            { Confederation.CAF, CafQuota },
            { Confederation.CONCACAF, ConcacafQuota },
            { Confederation.CONMEBOL, ConmebolQuota },
            { Confederation.OFC, OfcQuota },
            { Confederation.UEFA, UefaQuota }
        };
}
