using ProjectCopa;

namespace Services;

public class TeamServices
{
    private const int FixtureTeamCount = 48;
    private const int AfcQuota = 8;
    private const int CafQuota = 9;
    private const int OfcQuota = 1;
    private const int UefaQuota = 16;
    private const int ConmebolQuota = 7;
    private const int ConcacafQuota = 7;
    private const string TeamNotFoundMessage = "Team was not found.";
    private const string CannotDeleteTeamMessage = "Cannot delete team because it does not exist.";
    private const string DuplicateTeamNameMessage = "Team name must be unique in the system.";
    private const string QuotaExceededMessage = "{0} team quota exceeded.";
    private const string FixtureRequires48TeamsMessage = "Fixture ordering requires exactly 48 teams.";
    private static readonly IReadOnlyDictionary<Confederation, int> QuotasByConfederation = new Dictionary<Confederation, int>
    {
        { Confederation.AFC, AfcQuota },
        { Confederation.CAF, CafQuota },
        { Confederation.OFC, OfcQuota },
        { Confederation.UEFA, UefaQuota },
        { Confederation.CONMEBOL, ConmebolQuota },
        { Confederation.CONCACAF, ConcacafQuota }
    };

    private readonly List<Team> _equipos = new();

    public void AgregarEquipo(Team equipo)
    {
        ArgumentNullException.ThrowIfNull(equipo);
        ValidarNombreUnico(equipo);
        ValidarCupoConfederacion(equipo);

        _equipos.Add(equipo);
    }

    public IReadOnlyCollection<Team> ObtenerEquipos()
    {
        return _equipos.AsReadOnly();
    }

    public Team BuscarPorNombre(string nombre)
    {
        ArgumentNullException.ThrowIfNull(nombre);

        return _equipos.FirstOrDefault(equipo => equipo.Nombre == nombre)
            ?? throw new InvalidOperationException(TeamNotFoundMessage);
    }

    public void EliminarEquipo(string nombre)
    {
        ArgumentNullException.ThrowIfNull(nombre);

        var equipo = _equipos.FirstOrDefault(existingTeam => existingTeam.Nombre == nombre);

        if (equipo is null)
        {
            throw new InvalidOperationException(CannotDeleteTeamMessage);
        }

        _equipos.Remove(equipo);
    }

    public IReadOnlyList<Team> OrdenarParaFixture(IReadOnlyCollection<Team> equipos, int semillaFixture)
    {
        ArgumentNullException.ThrowIfNull(equipos);
        ValidarCantidadEquiposParaFixture(equipos.Count);

        return ConstruirOrdenFixture(equipos, semillaFixture);
    }

    private void ValidarNombreUnico(Team equipo)
    {
        if (ExisteNombreDuplicado(equipo.Nombre))
        {
            throw new InvalidOperationException(DuplicateTeamNameMessage);
        }
    }

    private void ValidarCupoConfederacion(Team equipo)
    {
        if (QuotasByConfederation.TryGetValue(equipo.Confederation, out var quota) &&
            CantidadEquiposPorConfederacion(equipo.Confederation) >= quota)
        {
            throw new InvalidOperationException(string.Format(QuotaExceededMessage, equipo.Confederation));
        }
    }

    private bool ExisteNombreDuplicado(string nombre)
    {
        return _equipos.Any(existingTeam => existingTeam.Nombre == nombre);
    }

    private int CantidadEquiposPorConfederacion(Confederation confederation)
    {
        return _equipos.Count(existingTeam => existingTeam.Confederation == confederation);
    }

    private static void FisherYatesDeterministico(IList<Team> equipos, int semilla)
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

    private static IReadOnlyList<Team> ConstruirOrdenFixture(IEnumerable<Team> equipos, int semillaFixture)
    {
        var ordenados = new List<Team>(capacity: FixtureTeamCount);
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




