using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public class FixtureServiceTests
{
   
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CannotCreateFixtureWithout48Teams()
    {
        var teams = new List<Team>();
        var stadiums = new List<Stadium>();

        FixtureService service = new FixtureService(teams, stadiums);

        service.CreateFixture();
    }

    [TestMethod]
    public void CreateFixtureWith48TeamsCreates12Groups()
    {
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();
        
        Assert.AreEqual(12, groups.Count);
    }

    private List<Team> Create48ValidTeams()
    {
        List<Team> teams = new();

        for (int i = 0; i < 48; i++)
        {
            teams.Add(new Team(
                "Team" + i,
                1000 + i,
                false,
                Confederation.UEFA
            ));
        }

        return teams;
    }

    private List<Stadium> Create4ValidStadiums()
    {
        return new List<Stadium>
        {
            new Stadium("Stadium1", "City", null, 30000),
            new Stadium("Stadium2", "City", null, 30000),
            new Stadium("Stadium3", "City", null, 30000),
            new Stadium("Stadium4", "City", null, 30000)
        };
    }

    
    [TestMethod]
    public void CreateFixtureDistributesTeamsInto12GroupsOf4()
    {
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();
        
        foreach (Group group in groups)
        {
            Assert.AreEqual(4, group.Teams.Count);
        }
    }


}