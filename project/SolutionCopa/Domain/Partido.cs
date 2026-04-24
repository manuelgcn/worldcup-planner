using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Partido
{
    public int IdPartido { get; private set; }
    public DateTime FechaPartido { get; private set; }

    private IEquipo? _equipoLocal;
    private IEquipo? _equipoVisita;

    public IEquipo? EquipoLocal
    {
        get => _equipoLocal;
        private set
        {
            ValidarEquiposDistintos(value, _equipoVisita);

            _equipoLocal = value;
        }
    }

    public IEquipo? EquipoVisita
    {
        get => _equipoVisita;
        private set
        {
            ValidarEquiposDistintos(_equipoLocal, value);

            _equipoVisita = value;
        }
    }

    public int GolLocal { get; private set; }
    public int GolVisita { get; private set; }
    public IEstadio? Estadio { get; private set; }

    public Partido()
    {
    }

    public Partido(int id, DateTime fechaPartido, IEquipo equipoLocal, IEquipo equipoVisita, IEstadio estadio)
    {
        IdPartido = id;
        FechaPartido = fechaPartido;
        EquipoLocal = equipoLocal;
        EquipoVisita = equipoVisita;
        GolLocal = 0;
        GolVisita = 0;
        Estadio = estadio;
    }

    private static void ValidarEquiposDistintos(IEquipo? equipoLocal, IEquipo? equipoVisita)
    {
        if (equipoLocal != null && equipoVisita != null && ReferenceEquals(equipoLocal, equipoVisita))
            throw new MatchException("El equipo local debe ser distinto al equipo de visita");
    }
}