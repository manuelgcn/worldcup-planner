using ProjectCopa;

namespace Services;

public class EquipoService
{
    private const int UefaQuota = 16;
    private const int ConmebolQuota = 7;
    private const int ConcacafQuota = 7;
    private static readonly IReadOnlyDictionary<Confederacion, int> QuotasByConfederation = new Dictionary<Confederacion, int>
    {
        { Confederacion.UEFA, UefaQuota },
        { Confederacion.CONMEBOL, ConmebolQuota },
        { Confederacion.CONCACAF, ConcacafQuota }
    };

    private readonly List<Equipo> _equipos = new();

    public void AgregarEquipo(Equipo equipo)
    {
        ValidarNombreUnico(equipo);
        ValidarCupoConfederacion(equipo);

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

    private void ValidarCupoConfederacion(Equipo equipo)
    {
        if (!QuotasByConfederation.TryGetValue(equipo.Confederacion, out var quota))
        {
            return;
        }

        if (CantidadEquiposPorConfederacion(equipo.Confederacion) >= quota)
        {
            throw new InvalidOperationException($"{equipo.Confederacion} team quota exceeded.");
        }
    }

    private bool ExisteNombreDuplicado(string nombre)
    {
        return _equipos.Any(existingTeam => existingTeam.Nombre == nombre);
    }

    private int CantidadEquiposPorConfederacion(Confederacion confederacion)
    {
        return _equipos.Count(existingTeam => existingTeam.Confederacion == confederacion);
    }
}


