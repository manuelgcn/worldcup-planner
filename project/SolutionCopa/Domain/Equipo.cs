namespace ProjectCopa;

public class Equipo
{
    private const int MaxNombreLength = 60;

    public string Nombre { get; }
    public int RankingFifa { get; }
    public bool EsFicticio { get; }
    public Confederacion Confederacion { get; }

    public Equipo(string nombre, int rankingFifa, bool esFicticio, Confederacion confederacion)
    {
        ValidarNombre(nombre);
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
}

