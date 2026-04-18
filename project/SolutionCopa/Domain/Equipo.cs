namespace ProjectCopa;

public class Equipo
{
    private const int MaxNombreLength = 60;
    private const int MinRankingFifa = 300;
    private const int MaxRankingFifa = 2500;

    public string Nombre { get; }
    public int RankingFifa { get; }
    public bool EsFicticio { get; }
    public Confederacion Confederacion { get; }

    public Equipo(string nombre, int rankingFifa, bool esFicticio, Confederacion confederacion)
    {
        ValidarNombre(nombre);
        ValidarRankingFifa(rankingFifa);
        Nombre = nombre;
        RankingFifa = rankingFifa;
        EsFicticio = esFicticio;
        Confederacion = confederacion;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.Length > MaxNombreLength)
        {
            throw new ArgumentException("El nombre del equipo no puede ser vacío.", nameof(nombre));
        }
    }

    private static void ValidarRankingFifa(int rankingFifa)
    {
        if (rankingFifa < MinRankingFifa || rankingFifa > MaxRankingFifa)
        {
            throw new ArgumentException($"El ranking FIFA debe estar entre {MinRankingFifa} y {MaxRankingFifa}.", nameof(rankingFifa));
        }
    }
}

