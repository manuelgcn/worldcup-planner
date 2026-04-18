namespace ProjectCopa;

public class Equipo
{
    public string Nombre { get; }
    public int RankingFifa { get; }

    public Equipo(string nombre, int rankingFifa, bool esFicticio, Confederacion confederacion)
    {
        if (string.IsNullOrEmpty(nombre))
        {
            throw new ArgumentException("El nombre del equipo no puede ser vacío.", nameof(nombre));
        }

        Nombre = nombre;
        RankingFifa = rankingFifa;
    }
}

