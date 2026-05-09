using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class TeamServices(ITeamRepository teamRepository, IUnitOfWork unitOfWork) : ITeamService
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

    public void AddTeam(Team team)
    {
        ArgumentNullException.ThrowIfNull(team);
        ValidateUniqueName(team.Name);
        ValidateConfederationQuota(team.Confederation);

        teamRepository.Add(team);
        unitOfWork.Commit();
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

    public void RemoveTeam(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Team team = teamRepository.GetByName(name)
            ?? throw new TeamServicesException(CannotDeleteTeamMessage);

        teamRepository.Delete(team);
        unitOfWork.Commit();
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
        var random = new Random(seed);

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




