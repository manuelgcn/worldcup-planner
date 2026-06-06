using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;
namespace Services;

public class FixtureService
{
    private const int GroupsInTournament = 12;
    private const int TeamsPerGroup = 4;
    private const int RequiredTeams = GroupsInTournament * TeamsPerGroup;
    private const int MaxUefaTeamsPerGroup = 2;
    private const int MaxNonUefaTeamsPerGroup = 1;

    // Each group draws exactly one team from each bombo, so there are as many bombos as teams per group.
    private const int BomboCount = TeamsPerGroup;

    private readonly IReadOnlyCollection<Team> teams;
    private readonly IReadOnlyCollection<Stadium> stadiums;
    private readonly IAuditLogService? _auditLogService;
    private readonly int seed;

    public FixtureService(
        IReadOnlyCollection<Team> teams,
        IReadOnlyCollection<Stadium> stadiums,
        int? seed = null,
        IAuditLogService? auditLogService = null)
    {
        this.teams = teams;
        this.stadiums = stadiums;
        this.seed = seed ?? 0;
        _auditLogService = auditLogService;
    }

    public IReadOnlyCollection<Group> CreateFixture()
    {
        ValidatePreconditions();

        List<Group> groups = CreateGroups();
        AssignTeamsToGroups(groups);
        _auditLogService?.Log(
            AuditActions.GenerateFixture,
            $"Fixture created with {groups.Count} groups and {teams.Count} teams using seed {seed}.",
            nameof(Group));

        return groups;
    }


    private void ValidatePreconditions()
    {
        if (teams.Count != RequiredTeams)
            throw new ArgumentException("Fixture requires exactly 48 teams");

        if (stadiums.Count < 4)
            throw new ArgumentException("At least 4 stadiums are required");
    }

    private List<Group> CreateGroups()
    {
        List<Group> groups = new List<Group>();

        for (char c = 'A'; c <= 'L'; c++)
        {
            groups.Add(new Group(c.ToString()));
        }

        return groups;
    }

    private void AssignTeamsToGroups(List<Group> groups)
    {
        Random random = new Random(seed);
        List<List<Team>> bombos = CreateBombos(random);

        List<List<Team>> assignments = new List<List<Team>>();
        for (int i = 0; i < groups.Count; i++)
            assignments.Add(new List<Team>());

        foreach (List<Team> bombo in bombos)
        {
            bool[] groupAlreadyTakenInBombo = new bool[groups.Count];
            if (!TryPlaceBomboOneTeamPerGroup(assignments, bombo, groupAlreadyTakenInBombo, 0))
            {
                throw new FixtureServiceException(
                    "Cannot build fixture: confederation quotas cannot be satisfied " +
                    "(max 1 team per non-UEFA confederation per group, max 2 UEFA per group).");
            }
        }

        for (int g = 0; g < groups.Count; g++)
        {
            foreach (Team team in assignments[g])
            {
                groups[g].AddTeam(team);
            }
        }
    }

    private List<List<Team>> CreateBombos(Random random)
    {
        List<Team> orderedTeams = OrderTeamsByRankingWithDeterministicTieBreak(random);

        List<List<Team>> bombos = new List<List<Team>>();

        for (int i = 0; i < BomboCount; i++)
        {
            bombos.Add(
                orderedTeams
                    .Skip(i * GroupsInTournament)
                    .Take(GroupsInTournament)
                    .ToList()
            );
        }

        // The composition of each bombo (which 12 teams it holds, by ranking) is fixed; FixtureSeed
        // only decides the draw order within each bombo, which is what makes the group assignment vary.
        foreach (List<Team> bombo in bombos)
        {
            ShuffleDeterministically(bombo, random);
        }

        return bombos;
    }

    private bool TryPlaceBomboOneTeamPerGroup(
        List<List<Team>> assignments,
        List<Team> bombo,
        bool[] groupAlreadyTakenInBombo,
        int teamIndex)
    {
        if (teamIndex >= bombo.Count) return true;

        Team team = bombo[teamIndex];

        for (int groupIndex = 0; groupIndex < assignments.Count; groupIndex++)
        {
            if (groupAlreadyTakenInBombo[groupIndex]) continue;
            if (!CanPlaceTeamInGroup(assignments[groupIndex], team)) continue;

            assignments[groupIndex].Add(team);
            groupAlreadyTakenInBombo[groupIndex] = true;

            if (TryPlaceBomboOneTeamPerGroup(assignments, bombo, groupAlreadyTakenInBombo, teamIndex + 1))
                return true;

            assignments[groupIndex].RemoveAt(assignments[groupIndex].Count - 1);
            groupAlreadyTakenInBombo[groupIndex] = false;
        }

        return false;
    }

    private static bool CanPlaceTeamInGroup(List<Team> groupTeams, Team team)
    {
        if (groupTeams.Count >= TeamsPerGroup) return false;

        int sameConfederationCount = groupTeams.Count(t => t.Confederation == team.Confederation);
        int maxAllowed = team.Confederation == Confederation.UEFA
            ? MaxUefaTeamsPerGroup
            : MaxNonUefaTeamsPerGroup;

        return sameConfederationCount < maxAllowed;
    }

    private List<Team> OrderTeamsByRankingWithDeterministicTieBreak(Random random)
    {
        return teams
            .GroupBy(t => t.FifaRanking)
            .OrderByDescending(g => g.Key)
            .SelectMany(group =>
            {
                List<Team> tiedTeams = group.ToList();
                ShuffleDeterministically(tiedTeams, random);
                return tiedTeams;
            })
            .ToList();
    }

    private void ShuffleDeterministically(List<Team> teams, Random random)
    {
        for (int i = teams.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (teams[i], teams[j]) = (teams[j], teams[i]);
        }
    }
}
