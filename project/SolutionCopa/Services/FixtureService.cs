using ProjectCopa;
namespace Services;

public class FixtureService
{
   
    private readonly IReadOnlyCollection<Team> teams;
    private readonly IReadOnlyCollection<Stadium> stadiums;

    
    
    private readonly int seed;

    
    public FixtureService(
        IReadOnlyCollection<Team> teams,
        IReadOnlyCollection<Stadium> stadiums,
        int? seed = null)
    {
        this.teams = teams;
        this.stadiums = stadiums;
        this.seed = seed ?? 0; 
    }




   
    public IReadOnlyCollection<Group> CreateFixture()
    {
        ValidatePreconditions();

        List<Group> groups = CreateGroups();
        AssignTeamsToGroups(groups);

        return groups;
    }


    private void ValidatePreconditions()
    {
        if (teams.Count != 48)
            throw new ArgumentException("Fixture requires exactly 48 teams");

        if (stadiums.Count < 4)
            throw new ArgumentException("At least 4 stadiums are required");
    }

    private List<Group> CreateGroups()
    {
        List<Group> groups = new();

        for (char c = 'A'; c <= 'L'; c++)
        {
            groups.Add(new Group(c.ToString()));
        }
        
        return groups;
    }


    
    
    private void AssignTeamsToGroups(List<Group> groups)
    {
        int groupIndex = 0;

        List<Team> orderedTeams = OrderTeamsByRankingWithDeterministicTieBreak();

        foreach (Team team in orderedTeams)
        {
            groups[groupIndex].AddTeam(team);
            groupIndex = (groupIndex + 1) % groups.Count;
        }
    }


    
    private List<Team> OrderTeamsByRankingWithDeterministicTieBreak()
    {
        Random random = new Random(seed);

        return teams
            .GroupBy(t => t.RankingFifa)
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