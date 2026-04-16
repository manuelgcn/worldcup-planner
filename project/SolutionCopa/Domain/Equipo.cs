namespace ProjectCopa;

public class Equipo
{
    public string Nombre { get; }

    public Equipo(string nombre, int rankingFifa, bool eliminado, Confederacion confederacion)
    {
        Nombre = nombre;
    }
}

