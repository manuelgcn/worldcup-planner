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
            if (value != null && ReferenceEquals(value, _equipoVisita))
                throw new ArgumentException("El equipo local debe ser distinto al equipo de visita");

            _equipoLocal = value;
        }
    }

    public IEquipo? EquipoVisita
    {
        get => _equipoVisita;
        private set
        {
            if (value != null && ReferenceEquals(value, _equipoLocal))
                throw new ArgumentException("El equipo de visita debe ser distinto al equipo local");

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
}