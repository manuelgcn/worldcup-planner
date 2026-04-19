using ProjectCopa;

namespace Services;

public class EquipoService
{
    private const int UefaQuota = 16;
    private const int ConmebolQuota = 7;
    private readonly List<Equipo> _equipos = new();

    public void AgregarEquipo(Equipo equipo)
    {
        ValidarNombreUnico(equipo);
        ValidarCupoUefa(equipo);
        ValidarCupoConmebol(equipo);

        _equipos.Add(equipo);
    }

    public IReadOnlyCollection<Equipo> ObtenerEquipos()
    {
        return _equipos.AsReadOnly();
    }

    private void ValidarNombreUnico(Equipo equipo)
    {
        if (ExisteNombreDuplicado(equipo.Nombre))
        {
            throw new InvalidOperationException("Team name must be unique in the system.");
        }
    }

    private void ValidarCupoUefa(Equipo equipo)
    {
        if (EsEquipoUefa(equipo) && CupoUefaCompleto())
        {
            throw new InvalidOperationException("UEFA team quota exceeded.");
        }
    }

    private void ValidarCupoConmebol(Equipo equipo)
    {
        if (EsEquipoConmebol(equipo) && CupoConmebolCompleto())
        {
            throw new InvalidOperationException("CONMEBOL team quota exceeded.");
        }
    }

    private bool ExisteNombreDuplicado(string nombre)
    {
        return _equipos.Any(existingTeam => existingTeam.Nombre == nombre);
    }

    private static bool EsEquipoUefa(Equipo equipo)
    {
        return equipo.Confederacion == Confederacion.UEFA;
    }

    private static bool EsEquipoConmebol(Equipo equipo)
    {
        return equipo.Confederacion == Confederacion.CONMEBOL;
    }

    private bool CupoUefaCompleto()
    {
        return _equipos.Count(existingTeam => existingTeam.Confederacion == Confederacion.UEFA) >= UefaQuota;
    }

    private bool CupoConmebolCompleto()
    {
        return _equipos.Count(existingTeam => existingTeam.Confederacion == Confederacion.CONMEBOL) >= ConmebolQuota;
    }
}


