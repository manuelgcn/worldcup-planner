using ProjectCopa;
using Services;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class FixtureServiceTests
{
   
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CannotCreateFixtureWithout48Teams()
    {
        List<Team> teams = new List<Team>();
        List<Stadium> stadiums = new List<Stadium>();

        FixtureService service = new FixtureService(teams, stadiums);

        service.CreateFixture();
    }

    [TestMethod]
    public void CannotCreateFixtureWithLessThanFourStadiums()
    {
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums =
        [
            new Stadium("Stadium1", "City", null, 30000),
            new Stadium("Stadium2", "City", null, 30000),
            new Stadium("Stadium3", "City", null, 30000)
        ];

        FixtureService service = new FixtureService(teams, stadiums);

        Assert.ThrowsException<ArgumentException>(() => service.CreateFixture());
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

    [TestMethod]
    public void CreateFixture_LogsAuditEntry()
    {
        List<Team> teams = Create48ValidTeams();
        List<Stadium> stadiums = Create4ValidStadiums();
        MockAuditLogRepository repository = new MockAuditLogRepository();
        FixtureService service = new FixtureService(teams, stadiums, auditLogService: AuditLogService.CreateInstance(repository, 1));

        service.CreateFixture();

        List<AuditLog> logs = repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("GENERATE_FIXTURE", logs[0].Action);
    }

    private List<Team> Create48ValidTeams()
    {
        return Create48TeamsWithRealisticConfederationQuotas();
    }

    private static List<Team> Create48TeamsWithRealisticConfederationQuotas()
    {
        List<Team> teams = new List<Team>();
        int id = 1;
        int ranking = 2000;

        (Confederation conf, int count)[] quotas =
        {
            (Confederation.UEFA, 16),
            (Confederation.CONMEBOL, 7),
            (Confederation.CONCACAF, 7),
            (Confederation.CAF, 9),
            (Confederation.AFC, 8),
            (Confederation.OFC, 1)
        };

        foreach ((Confederation conf, int count) in quotas)
        {
            for (int i = 0; i < count; i++)
            {
                teams.Add(new Team(
                    id,
                    conf + "_" + (i + 1).ToString("00"),
                    ranking,
                    false,
                    conf
                ));
                id++;
                ranking--;
            }
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

    private static List<string> DescribeGroupAssignments(IReadOnlyCollection<Group> groups)
    {
        return groups
            .Select(group => group.Name + ":" + string.Join(",", group.Teams.Select(team => team.Name)))
            .ToList();
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
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        Team highRank = new Team(99, "HighRank", 2500, false, Confederation.UEFA);
        teams.RemoveAt(0);
        teams.Insert(0, highRank);

        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        Group firstGroup = groups.First();

        Assert.AreEqual("HighRank", firstGroup.Teams.First().Name);
    }


    
    [TestMethod]
    public void CreateFixtureResolvesEqualRankingsDeterministically()
    {
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();

        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service1 = new FixtureService(teams, stadiums, seed: 42);
        FixtureService service2 = new FixtureService(teams, stadiums, seed: 42);

        IReadOnlyCollection<Group> groups1 = service1.CreateFixture();
        IReadOnlyCollection<Group> groups2 = service2.CreateFixture();

        List<string> firstTeamsRun1 = groups1.Select(g => g.Teams.First().Name).ToList();
        List<string> firstTeamsRun2 = groups2.Select(g => g.Teams.First().Name).ToList();

        CollectionAssert.AreEqual(firstTeamsRun1, firstTeamsRun2);
    }

    
    [TestMethod]
    public void CreateFixtureDistributesOneTeamFromEachBomboIntoEachGroup()
    {
        List<Team> teams = new List<Team>();
        int id = 1;

        (Confederation conf, int count)[] quotas =
        {
            (Confederation.UEFA, 16),
            (Confederation.CONMEBOL, 7),
            (Confederation.CONCACAF, 7),
            (Confederation.CAF, 9),
            (Confederation.AFC, 8),
            (Confederation.OFC, 1)
        };

        int bomboBase = 2400;
        int teamIndexInBombo = 0;
        int currentBombo = 0;
        int placedInQuota = 0;
        int quotaIdx = 0;

        for (int i = 0; i < 48; i++)
        {
            if (teamIndexInBombo == 12)
            {
                teamIndexInBombo = 0;
                currentBombo++;
                bomboBase -= 400;
            }

            while (placedInQuota == quotas[quotaIdx].count)
            {
                quotaIdx++;
                placedInQuota = 0;
            }

            teams.Add(new Team(
                id++,
                "B" + (currentBombo + 1) + "_" + quotas[quotaIdx].conf + "_" + (i + 1),
                bomboBase - teamIndexInBombo,
                false,
                quotas[quotaIdx].conf
            ));

            placedInQuota++;
            teamIndexInBombo++;
        }

        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        foreach (Group group in groups)
        {
            List<int> rankings = group.Teams
                .Select(t => t.FifaRanking)
                .OrderByDescending(r => r)
                .ToList();

            Assert.IsTrue(rankings[0] >= 2300);
            Assert.IsTrue(rankings[1] >= 1900 && rankings[1] <= 2100);
            Assert.IsTrue(rankings[2] >= 1500 && rankings[2] <= 1700);
            Assert.IsTrue(rankings[3] <= 1300);
        }
    }

    
    [TestMethod]
    public void CreateFixtureAllowsAtMostTwoUefaTeamsPerGroup()
    {
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        foreach (Group group in groups)
        {
            int uefaCount = group.Teams.Count(t => t.Confederation == Confederation.UEFA);

            Assert.IsTrue(
                uefaCount <= 2,
                $"Group {group.Name} has more than 2 UEFA teams"
            );
        }
    }

    [TestMethod]
    public void CreateFixtureAllowsAtMostOneTeamPerNonUefaConfederationPerGroup()
    {
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        foreach (Group group in groups)
        {
            foreach (Confederation conf in Enum.GetValues<Confederation>())
            {
                if (conf == Confederation.UEFA) continue;

                int count = group.Teams.Count(t => t.Confederation == conf);
                Assert.IsTrue(
                    count <= 1,
                    $"Group {group.Name} has {count} teams from {conf} (max allowed is 1)"
                );
            }
        }
    }

    [TestMethod]
    public void CreateFixtureRespectsConfederationQuotasAndStillProduces12GroupsOf4()
    {
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        List<Stadium> stadiums = Create4ValidStadiums();

        FixtureService service = new FixtureService(teams, stadiums, seed: 7);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        Assert.AreEqual(12, groups.Count);
        foreach (Group group in groups)
        {
            Assert.AreEqual(4, group.Teams.Count);
        }

        List<Team> allTeams = groups.SelectMany(g => g.Teams).ToList();
        Assert.AreEqual(48, allTeams.Distinct().Count());
    }

    [TestMethod]
    public void CreateFixtureProducesSameAssignmentForSameSeed()
    {
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        List<Stadium> stadiums = Create4ValidStadiums();

        IReadOnlyCollection<Group> run1 = new FixtureService(teams, stadiums, seed: 123).CreateFixture();
        IReadOnlyCollection<Group> run2 = new FixtureService(teams, stadiums, seed: 123).CreateFixture();

        List<string> assignment1 = run1
            .Select(g => g.Name + ":" + string.Join(",", g.Teams.Select(t => t.Name)))
            .ToList();
        List<string> assignment2 = run2
            .Select(g => g.Name + ":" + string.Join(",", g.Teams.Select(t => t.Name)))
            .ToList();

        CollectionAssert.AreEqual(assignment1, assignment2);
    }

    [TestMethod]
    public void CreateFixtureWithDifferentSeedsProducesDifferentGroups()
    {
        // 48 teams with all-distinct rankings: there are no ranking ties, so the only thing the seed
        // can influence is the order in which each bombo is drawn. If that is not seeded, every seed
        // yields identical groups (the bug this fix targets).
        List<Team> teams = Create48TeamsWithRealisticConfederationQuotas();
        List<Stadium> stadiums = Create4ValidStadiums();

        List<string> seedZero = DescribeGroupAssignments(
            new FixtureService(teams, stadiums, seed: 0).CreateFixture());
        List<string> seedOne = DescribeGroupAssignments(
            new FixtureService(teams, stadiums, seed: 1).CreateFixture());

        CollectionAssert.AreNotEqual(
            seedZero,
            seedOne,
            "Different FixtureSeed values must produce different group assignments.");
    }

    [TestMethod]
    public void CreateFixtureDoesNotPlaceTwoNonUefaTeamsFromSameConfederationInGroupZero()
    {
        // Distribution crafted so that, with the old "bombo[i] → groups[i]" assignment,
        // two CONMEBOL teams (at rank positions 12 and 24) both end up in group A.
        List<Team> teams = new List<Team>();
        int id = 1;
        int rank = 2000;

        Confederation[] order = new Confederation[48];
        for (int i = 0; i < 12; i++) order[i] = Confederation.UEFA;
        for (int i = 12; i < 18; i++) order[i] = Confederation.CONMEBOL;
        for (int i = 18; i < 22; i++) order[i] = Confederation.UEFA;
        order[22] = Confederation.CAF;
        order[23] = Confederation.CAF;
        order[24] = Confederation.CONMEBOL;
        for (int i = 25; i < 32; i++) order[i] = Confederation.CAF;
        for (int i = 32; i < 39; i++) order[i] = Confederation.CONCACAF;
        for (int i = 39; i < 47; i++) order[i] = Confederation.AFC;
        order[47] = Confederation.OFC;

        for (int i = 0; i < 48; i++)
        {
            teams.Add(new Team(id++, order[i] + "_" + i, rank--, false, order[i]));
        }

        List<Stadium> stadiums = Create4ValidStadiums();
        FixtureService service = new FixtureService(teams, stadiums);

        IReadOnlyCollection<Group> groups = service.CreateFixture();

        foreach (Group group in groups)
        {
            int conmebolCount = group.Teams.Count(t => t.Confederation == Confederation.CONMEBOL);
            Assert.IsTrue(
                conmebolCount <= 1,
                $"Group {group.Name} has {conmebolCount} CONMEBOL teams (max 1)"
            );
        }
    }

    [TestMethod]
    public void CreateFixtureThrowsServiceExceptionWhenConfederationQuotasCannotBeSatisfied()
    {
        List<Team> teams = new List<Team>();

        for (int i = 0; i < 14; i++)
        {
            teams.Add(new Team(i + 1, "CAF_" + i, 2000 - i, false, Confederation.CAF));
        }
        for (int i = 0; i < 16; i++)
        {
            teams.Add(new Team(15 + i, "UEFA_" + i, 1900 - i, false, Confederation.UEFA));
        }
        for (int i = 0; i < 18; i++)
        {
            teams.Add(new Team(31 + i, "AFC_" + i, 1800 - i, false, Confederation.AFC));
        }

        List<Stadium> stadiums = Create4ValidStadiums();
        FixtureService service = new FixtureService(teams, stadiums);

        Assert.ThrowsException<FixtureServiceException>(() => service.CreateFixture());
    }
}
