namespace ProjectCopa;

public enum Confederacion
{
    CONMEBOL
}

public class Equipo
{
    public string Nombre { get; }

    public Equipo(string nombre, int rankingFifa, bool esFicticio, Confederacion confederacion)
    {
        Nombre = nombre;
    }
}