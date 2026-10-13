using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class TeamServices : ITeamService
{
    private readonly ITeamRepository teamRepository;
    private readonly IGroupRepository groupRepository;
    private readonly IAuditLogService auditLogService;

    private const int FixtureTeamCount = 48;
    private const int AfcQuota = 8;
    private const int CafQuota = 9;
    private const int OfcQuota = 1;
    private const int UefaQuota = 16;
    private const int ConmebolQuota = 7;
    private const int ConcacafQuota = 7;

    private const string TeamNotFoundMessage = "No se encontro el equipo.";
    private const string CannotDeleteTeamMessage = "No se puede eliminar el equipo porque no existe.";
    private const string TeamInUseMessage = "No se puede eliminar el equipo porque ya esta asignado a un grupo.";
    private const string DuplicateTeamNameMessage = "El nombre del equipo debe ser unico en el sistema.";
    private const string QuotaExceededMessage = "Se supero el cupo de equipos de {0}.";
    private const string FixtureRequires48TeamsMessage = "El ordenamiento del fixture requiere exactamente 48 equipos.";

    private static readonly IReadOnlyDictionary<Confederation, int> QuotasByConfederation =
        new Dictionary<Confederation, int>
        {
            { Confederation.AFC, AfcQuota },
            { Confederation.CAF, CafQuota },
            { Confederation.OFC, OfcQuota },
            { Confederation.UEFA, UefaQuota },
            { Confederation.CONMEBOL, ConmebolQuota },
            { Confederation.CONCACAF, ConcacafQuota }
        };

    // Orden fijo de recorrido de confederaciones. El orden de iteracion de un
    // Dictionary no esta garantizado por el lenguaje; fijarlo asegura que, a igual
    // semilla, los equipos ficticios y sus rankings sean reproducibles entre maquinas.
    private static readonly IReadOnlyList<Confederation> ConfederationFillOrder =
        new[]
        {
            Confederation.AFC,
            Confederation.CAF,
            Confederation.OFC,
            Confederation.UEFA,
            Confederation.CONMEBOL,
            Confederation.CONCACAF
        };

    public TeamServices(
        ITeamRepository teamRepository,
        IGroupRepository groupRepository,
        IAuditLogService auditLogService)
    {
        ArgumentNullException.ThrowIfNull(teamRepository);
        ArgumentNullException.ThrowIfNull(groupRepository);
        ArgumentNullException.ThrowIfNull(auditLogService);

        this.teamRepository = teamRepository;
        this.groupRepository = groupRepository;
        this.auditLogService = auditLogService;
    }

    public void FillToFortyEight(int? completionSeed, string? performedByEmail)
    {
        IReadOnlyCollection<Team> existing = teamRepository.GetAll();
        if (existing.Count >= FixtureTeamCount)
        {
            return;
        }

        int seed = completionSeed ?? 0;
        Random random = new Random(seed);
        int nextId = NextAvailableTeamId(existing);

        foreach (Confederation confederation in ConfederationFillOrder)
        {
            int quota = QuotasByConfederation[confederation];
            int existingForConfederation = teamRepository.CountByConfederation(confederation);
            int missing = quota - existingForConfederation;

            for (int sequence = 0; sequence < missing; sequence++)
            {
                Team fictitious = CreateFictitiousTeam(
                    nextId,
                    confederation,
                    sequence + existingForConfederation + 1,
                    random);

                teamRepository.Add(fictitious);

                auditLogService.LogPerformedBy(
                    AuditActions.CreateTeam,
                    $"Generated fictitious team '{fictitious.Name}' with ranking {fictitious.FifaRanking}.",
                    performedByEmail,
                    nameof(Team),
                    fictitious.Id);

                nextId++;
            }
        }
    }

    private static int NextAvailableTeamId(IReadOnlyCollection<Team> existing)
    {
        return existing.Count == 0 ? 1 : existing.Max(team => team.Id) + 1;
    }

    private static Team CreateFictitiousTeam(int id, Confederation confederation, int sequenceInConfederation, Random random)
    {
        const int MinRanking = 300;
        const int MaxRanking = 2500;

        int ranking = random.Next(MinRanking, MaxRanking + 1);
        string name = $"{confederation}_{sequenceInConfederation:D2}";

        return new Team(id, name, ranking, true, confederation);
    }

    public void AddTeam(Team team, string? performedByEmail = null)
    {
        ArgumentNullException.ThrowIfNull(team);

        ValidateUniqueName(team.Name);
        ValidateConfederationQuota(team.Confederation);

        teamRepository.Add(team);

        auditLogService.LogPerformedBy(
            AuditActions.CreateTeam,
            $"Team '{team.Name}' added with ranking {team.FifaRanking}.",
            performedByEmail,
            nameof(Team),
            team.Id);
    }

    public IReadOnlyCollection<Team> GetTeams()
    {
        return teamRepository.GetAll();
    }

    public Team FindByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return teamRepository.GetByName(name)
            ?? throw new TeamServicesException(TeamNotFoundMessage);
    }

    public void RemoveTeam(string name, string? performedByEmail = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        Team team = teamRepository.GetByName(name)
            ?? throw new TeamServicesException(CannotDeleteTeamMessage);

        bool teamBelongsToAGroup = groupRepository
            .GetAll()
            .Any(group => group.Teams.Any(t => t.Id == team.Id));

        if (teamBelongsToAGroup)
        {
            throw new TeamServicesException(TeamInUseMessage);
        }

        teamRepository.Delete(team);

        auditLogService.LogPerformedBy(
            AuditActions.DeleteTeam,
            $"Team '{team.Name}' removed.",
            performedByEmail,
            nameof(Team),
            team.Id);
    }

    public IReadOnlyList<Team> OrderForFixture(IReadOnlyCollection<Team> teams, int fixtureSeed)
    {
        ArgumentNullException.ThrowIfNull(teams);

        ValidateFixtureTeamCount(teams.Count);

        return BuildFixtureOrder(teams, fixtureSeed);
    }

    private void ValidateUniqueName(string name)
    {
        if (teamRepository.ExistsByName(name))
        {
            throw new TeamServicesException(DuplicateTeamNameMessage);
        }
    }

    private void ValidateConfederationQuota(Confederation confederation)
    {
        if (QuotasByConfederation.TryGetValue(confederation, out int quota) &&
            teamRepository.CountByConfederation(confederation) >= quota)
        {
            throw new TeamServicesException(string.Format(QuotaExceededMessage, confederation));
        }
    }

    private static void ShuffleDeterministically(IList<Team> teams, int seed)
    {
        Random random = new Random(seed);

        for (int i = teams.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (teams[i], teams[j]) = (teams[j], teams[i]);
        }
    }

    private static void ValidateFixtureTeamCount(int teamCount)
    {
        if (teamCount != FixtureTeamCount)
        {
            throw new TeamServicesException(FixtureRequires48TeamsMessage);
        }
    }

    private static IReadOnlyList<Team> BuildFixtureOrder(IEnumerable<Team> teams, int fixtureSeed)
    {
        List<Team> orderedTeams = new List<Team>(capacity: FixtureTeamCount);

        List<IGrouping<int, Team>> groupsByRanking = teams
            .GroupBy(team => team.FifaRanking)
            .OrderByDescending(group => group.Key)
            .ToList();

        int groupIndex = 0;

        foreach (IGrouping<int, Team> tiedGroup in groupsByRanking)
        {
            List<Team> tiedTeams = tiedGroup.ToList();
            ShuffleDeterministically(tiedTeams, fixtureSeed + groupIndex);
            orderedTeams.AddRange(tiedTeams);
            groupIndex++;
        }

        return orderedTeams;
    }
}