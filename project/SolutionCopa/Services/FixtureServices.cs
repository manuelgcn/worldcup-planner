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

    
    public IReadOnlyCollection<Group> CreateFixture()
    {
        ValidatePreconditions();

        var groups = CreateGroups();
        
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

}