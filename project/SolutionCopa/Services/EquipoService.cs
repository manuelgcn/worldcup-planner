using ProjectCopa;

namespace Services;

public class EquipoService
{
    private const int UefaQuota = 16;
    private readonly List<Equipo> _equipos = new();

    public void AgregarEquipo(Equipo equipo)
    {
        if (_equipos.Any(existingTeam => existingTeam.Nombre == equipo.Nombre))
        {
            throw new InvalidOperationException("Team name must be unique in the system.");
        }

        if (equipo.Confederacion == Confederacion.UEFA && _equipos.Count(existingTeam => existingTeam.Confederacion == Confederacion.UEFA) >= UefaQuota)
        {
            throw new InvalidOperationException("UEFA team quota exceeded.");
        }

        _equipos.Add(equipo);
    }

    public IReadOnlyCollection<Equipo> ObtenerEquipos()
    {
        return _equipos.AsReadOnly();
    }
}


