namespace ProjectCopa;

public class Team
{
    private const int MaxNombreLength = 60;
    private const int MinRankingFifa = 300;
    private const int MaxRankingFifa = 2500;
    private const string NombreInvalidoMessage = "El nombre del equipo no puede ser vacío.";
    private const string RankingFueraDeRangoMessage = "El ranking FIFA debe estar entre {0} y {1}.";

    public string Nombre { get; }
    public int RankingFifa { get; }
    public bool EsFicticio { get; }
    public Confederation Confederation { get; }

    public Team(string nombre, int rankingFifa, bool esFicticio, Confederation confederation)
    {
        ValidarNombre(nombre);
        ValidarRankingFifa(rankingFifa);
        Nombre = nombre;
        RankingFifa = rankingFifa;
        EsFicticio = esFicticio;
        Confederation = confederation;
    }

    private static void ValidarNombre(string nombre)
    {
        if (NombreEsInvalido(nombre))
        {
            throw new ArgumentException(NombreInvalidoMessage, nameof(nombre));
        }
    }

    private static void ValidarRankingFifa(int rankingFifa)
    {
        if (RankingFifaFueraDeRango(rankingFifa))
        {
            throw new ArgumentException(string.Format(RankingFueraDeRangoMessage, MinRankingFifa, MaxRankingFifa), nameof(rankingFifa));
        }
    }

    private static bool NombreEsInvalido(string nombre)
    {
        return string.IsNullOrEmpty(nombre) || nombre.Length > MaxNombreLength;
    }

    private static bool RankingFifaFueraDeRango(int rankingFifa)
    {
        return rankingFifa < MinRankingFifa || rankingFifa > MaxRankingFifa;
    }
}


