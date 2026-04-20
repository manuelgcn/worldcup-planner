using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Partido
{
    public int idPartido { get; private set; }
    public DateTime fechaPartido { get; private set; }
    public IEquipo? equipoLocal { get; private set; }
    public IEquipo? equipoVisita { get; private set; }
    public int golLocal { get; private set; }
    public int golVisita { get; private set; }
    public IEstadio? estadio { get; private set; }

    public Partido()
    {
    }

    public Partido(int id, DateTime fechaPartido, IEquipo equipoLocal, IEquipo equipoVisita, int golLocal, int golVisita, IEstadio estadio)
    {
        this.idPartido = id;
        this.fechaPartido = fechaPartido;
        this.equipoLocal = equipoLocal;
        this.equipoVisita = equipoVisita;
        this.golLocal = golLocal;
        this.golVisita = golVisita;
        this.estadio = estadio;
    }
}