namespace ProjectCopa;

public class Equipo
{
    public string Nombre { get; }
    public int RankingFifa { get; }
    
    public bool EsFicticio { get; }
    
    public Confederacion Confederacion { get; }

    public Equipo(string nombre, int rankingFifa, bool esFicticio, Confederacion confederacion)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.Length > 60)
        {
            throw new ArgumentException("El nombre del equipo no puede ser vacío.", nameof(nombre));
        }

        Nombre = nombre;
        RankingFifa = rankingFifa;
        EsFicticio = esFicticio;
        Confederacion = confederacion;
    }
}

