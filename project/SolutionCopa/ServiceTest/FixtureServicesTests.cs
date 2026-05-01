using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public class FixtureServicesTests
{
   
    
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CannotCreateFixtureWithout48Teams()
    {
        var teams = new List<Team>();
        var stadiums = new List<Stadium>();

        FixtureServices service = new FixtureServices(teams, stadiums);

        service.CreateFixture();
    }

    
    [TestMethod]
    public void CreateFixtureWith48TeamsCreates12Groups()
    {
        
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureServices service = new FixtureServices(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();
        
        Assert.AreEqual(12, groups.Count);
    }



}