namespace ProjectCopa;

// Fuente unica de verdad del desempate por sorteo de la fase de grupos (H8). Tanto la
// clasificacion (GroupQualifier) como la tabla visible (GroupStandingsService) deben usar
// este mismo calculo determinista por-equipo para que, ante un empate exacto (Pts/DG/GF),
// ambos ordenen identico con la misma CrossingsSeed.
public static class GroupTieBreaker
{
    public static int Compute(int seed, int groupIndex, int teamId)
    {
        int mixedSeed = unchecked((seed * 397) ^ (groupIndex * 31) ^ teamId);
        return new Random(mixedSeed).Next();
    }
}
