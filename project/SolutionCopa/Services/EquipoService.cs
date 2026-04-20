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
    private const string TeamNotFoundMessage = "Team was not found.";
    private const string DuplicateTeamNameMessage = "Team name must be unique in the system.";
    private const string QuotaExceededMessage = "{0} team quota exceeded.";
    private const string FixtureRequires48TeamsMessage = "Fixture ordering requires exactly 48 teams.";
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
        ArgumentNullException.ThrowIfNull(equipo);
        ValidarNombreUnico(equipo);
        ValidarCupoConfederacion(equipo);

        _equipos.Add(equipo);
    }

    public IReadOnlyCollection<Equipo> ObtenerEquipos()
    {
        return _equipos.AsReadOnly();
    }

    public Equipo BuscarPorNombre(string nombre)
    {
        ArgumentNullException.ThrowIfNull(nombre);

        return _equipos.FirstOrDefault(equipo => equipo.Nombre == nombre)
            ?? throw new InvalidOperationException(TeamNotFoundMessage);
    }

    public void EliminarEquipo(string nombre)
    {
        var equipo = BuscarPorNombre(nombre);
        _equipos.Remove(equipo);
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
            throw new InvalidOperationException(DuplicateTeamNameMessage);
        }
    }

    private void ValidarCupoConfederacion(Equipo equipo)
    {
        if (QuotasByConfederation.TryGetValue(equipo.Confederacion, out var quota) &&
            CantidadEquiposPorConfederacion(equipo.Confederacion) >= quota)
        {
            throw new InvalidOperationException(string.Format(QuotaExceededMessage, equipo.Confederacion));
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
            throw new InvalidOperationException(FixtureRequires48TeamsMessage);
        }
    }

    private static IReadOnlyList<Equipo> ConstruirOrdenFixture(IEnumerable<Equipo> equipos, int semillaFixture)
    {
        var ordenados = new List<Equipo>(capacity: FixtureTeamCount);
        var gruposPorRanking = equipos
            .GroupBy(equipo => equipo.RankingFifa)
            .OrderByDescending(grupo => grupo.Key)
            .ToList();

        var indiceGrupo = 0;

        foreach (var grupoEmpatado in gruposPorRanking)
        {
            var equiposEmpatados = grupoEmpatado.ToList();
            FisherYatesDeterministico(equiposEmpatados, semillaFixture + indiceGrupo);
            ordenados.AddRange(equiposEmpatados);
            indiceGrupo++;
        }

        return ordenados;
    }
}



