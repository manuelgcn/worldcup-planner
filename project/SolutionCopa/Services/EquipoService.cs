using ProjectCopa;

namespace Services;

public class EquipoService
{
    private readonly List<Equipo> _equipos = new();

    public void AgregarEquipo(Equipo equipo)
    {
        if (_equipos.Any(existingTeam => existingTeam.Nombre == equipo.Nombre))
        {
            throw new InvalidOperationException("Team name must be unique in the system.");
        }

        _equipos.Add(equipo);
    }

    public IReadOnlyCollection<Equipo> ObtenerEquipos()
    {
        return _equipos.AsReadOnly();
    }
}


