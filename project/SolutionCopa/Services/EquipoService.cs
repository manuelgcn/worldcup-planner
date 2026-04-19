using ProjectCopa;

namespace Services;

public class EquipoService
{
    private const int FixtureTeamCount = 48;
    private const int AfcQuota = 8;
    private const int CafQuota = 9;
    private const int OfcQuota = 1;
    private const int UefaQuota = 16;
    private const int ConmebolQuota = 7;
    private const int ConcacafQuota = 7;
    private static readonly IReadOnlyDictionary<Confederacion, int> QuotasByConfederation = new Dictionary<Confederacion, int>
    {
        { Confederacion.AFC, AfcQuota },
        { Confederacion.CAF, CafQuota },
        { Confederacion.OFC, OfcQuota },
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

    public IReadOnlyList<Equipo> OrdenarParaFixture(IReadOnlyCollection<Equipo> equipos, int semillaFixture)
    {
        ArgumentNullException.ThrowIfNull(equipos);
        ValidarCantidadEquiposParaFixture(equipos.Count);

        return ConstruirOrdenFixture(equipos, semillaFixture);
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

    private static void FisherYatesDeterministico(IList<Equipo> equipos, int semilla)
    {
        var random = new Random(semilla);

        for (var i = equipos.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (equipos[i], equipos[j]) = (equipos[j], equipos[i]);
        }
    }

    private static void ValidarCantidadEquiposParaFixture(int cantidadEquipos)
    {
        if (cantidadEquipos != FixtureTeamCount)
        {
            throw new InvalidOperationException("Fixture ordering requires exactly 48 teams.");
        }
    }

    private static IReadOnlyList<Equipo> ConstruirOrdenFixture(IEnumerable<Equipo> equipos, int semillaFixture)
    {
        var ordenados = new List<Equipo>(capacity: FixtureTeamCount);
        var gruposPorRanking = equipos
            .GroupBy(equipo => equipo.RankingFifa)
            .OrderByDescending(grupo => grupo.Key)
            .ToList();

        for (var indiceGrupo = 0; indiceGrupo < gruposPorRanking.Count; indiceGrupo++)
        {
            var grupoEmpatado = gruposPorRanking[indiceGrupo].ToList();
            FisherYatesDeterministico(grupoEmpatado, semillaFixture + indiceGrupo);
            ordenados.AddRange(grupoEmpatado);
        }

        return ordenados;
    }
}


