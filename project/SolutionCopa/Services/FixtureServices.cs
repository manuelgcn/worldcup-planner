using ProjectCopa;
namespace Services;

public class FixtureServices
{
    
   
    private readonly IReadOnlyCollection<Team> teams;
    private readonly IReadOnlyCollection<Stadium> stadiums;

    public FixtureServices(
        IReadOnlyCollection<Team> teams,
        IReadOnlyCollection<Stadium> stadiums)
    {
        this.teams = teams;
        this.stadiums = stadiums;
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

}