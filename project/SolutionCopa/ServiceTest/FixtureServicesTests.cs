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



}