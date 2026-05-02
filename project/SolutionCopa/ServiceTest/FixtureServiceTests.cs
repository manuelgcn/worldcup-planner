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


    [TestMethod]
    public void CreateFixtureUsesTeamOnce()
    {
        
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();
        
        List<Team> allTeamsInGroups = groups
            .SelectMany(g => g.Teams)
            .ToList();
        
        Assert.AreEqual(48, allTeamsInGroups.Count);
        
        Assert.AreEqual(
            48,
            allTeamsInGroups.Distinct().Count()
        );
    }
    
    
    [TestMethod]
    public void CreateFixtureAssignsTeamsByDescendingFifaRanking()
    {
        List<Team> teams = new List<Team>
        {
            new Team("MidRank", 1500, false, Confederation.UEFA),
            new Team("HighRank", 2000, false, Confederation.UEFA),
            new Team("LowRank", 1000, false, Confederation.UEFA)
        };
        
        for (int i = 3; i < 48; i++)
        {
            teams.Add(new Team(
                "Team" + i,
                500,
                false,
                Confederation.UEFA
            ));
        }

        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();

        Group firstGroup = groups.First();
        
        Assert.AreEqual("HighRank", firstGroup.Teams.First().Nombre);
    }


    
    [TestMethod]
    public void CreateFixtureResolvesEqualRankingsDeterministically()
    {
      
        List<Team> teams = new List<Team>();

        
        for (int i = 0; i < 12; i++)
        {
            teams.Add(new Team(
                "TeamEqual" + i,
                1500,
                false,
                Confederation.UEFA
            ));
        }

       
        for (int i = 12; i < 48; i++)
        {
            teams.Add(new Team(
                "Team" + i,
                1000,
                false,
                Confederation.UEFA
            ));
        }

        List<Stadium> stadiums = Create4ValidStadiums();

        
        FixtureService service1 = new FixtureService(teams, stadiums, seed: 42);
        FixtureService service2 = new FixtureService(teams, stadiums, seed: 42);

      
        var groups1 = service1.CreateFixture();
        var groups2 = service2.CreateFixture();

        var firstTeamsRun1 = groups1.Select(g => g.Teams.First().Nombre).ToList();
        var firstTeamsRun2 = groups2.Select(g => g.Teams.First().Nombre).ToList();

        
        CollectionAssert.AreEqual(firstTeamsRun1, firstTeamsRun2);
    }

    
    [TestMethod]
    public void CreateFixtureDistributesOneTeamFromEachBomboIntoEachGroup()
    {
       
        List<Team> teams = new List<Team>();

        // Bombo 1 (rankings mas altos)
        for (int i = 0; i < 12; i++)
        {
            teams.Add(new Team(
                "B1_Team" + i,
                2400 - i,
                false,
                Confederation.UEFA
            ));
        }

        // Bombo 2
        for (int i = 0; i < 12; i++)
        {
            teams.Add(new Team(
                "B2_Team" + i,
                2000 - i,
                false,
                Confederation.UEFA
            ));
        }

        // Bombo 4
        for (int i = 0; i < 12; i++)
        {
            teams.Add(new Team(
                "B4_Team" + i,
                1200 - i,
                false,
                Confederation.UEFA
            ));
        }
        
        // Bombo 3
        for (int i = 0; i < 12; i++)
        {
            teams.Add(new Team(
                "B3_Team" + i,
                1600 - i,
                false,
                Confederation.UEFA
            ));
        }

       

        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);
        
        IReadOnlyCollection<Group> groups = service.CreateFixture();
        
        foreach (Group group in groups)
        {
            var rankings = group.Teams
                .Select(t => t.RankingFifa)
                .OrderByDescending(r => r)
                .ToList();

            // cada grupo debe tener:
            // 1 equipo >= 2300 (bombo 1)
            // 1 equipo entre 1900-2100 (bombo 2)
            // 1 equipo entre 1500-1700 (bombo 3)
            // 1 equipo <= 1300 (bombo 4)
            Assert.IsTrue(rankings[0] >= 2300);
            Assert.IsTrue(rankings[1] >= 1900 && rankings[1] <= 2100);
            Assert.IsTrue(rankings[2] >= 1500 && rankings[2] <= 1700);
            Assert.IsTrue(rankings[3] <= 1300);
        }
    }

    
}