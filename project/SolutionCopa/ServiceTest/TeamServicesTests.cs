using ProjectCopa;
using Services;

namespace ServiceTest;

[TestClass]
public sealed class TeamServicesTests
{
    private TeamServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new TeamServices();
    }

    [TestMethod]
    public void CanAddTeam()
    {
        var team = new ProjectCopa.Team("Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);

        Assert.AreEqual(1, _service.GetTeams().Count);
    }

    [TestMethod]
    public void CannotAddDuplicateTeamName()
    {
        var t1 = new ProjectCopa.Team("Uruguay", 2000, false, Confederation.CONMEBOL);
        var t2 = new ProjectCopa.Team("Uruguay", 1800, false, Confederation.CONMEBOL);

        _service.AddTeam(t1);

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(t2));
    }

    [TestMethod]
    public void CannotAddTeamWithEmptyName()
    {
        Assert.ThrowsException<ArgumentException>(() => new ProjectCopa.Team("", 2000, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithLongName()
    {
        string longName = new string('A', 61);

        Assert.ThrowsException<ArgumentException>(() => new ProjectCopa.Team(longName, 2000, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithLowFifaRanking()
    {
        Assert.ThrowsException<ArgumentException>(() => new ProjectCopa.Team("Uruguay", 299, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CannotAddTeamWithHighFifaRanking()
    {
        Assert.ThrowsException<ArgumentException>(() => new ProjectCopa.Team("Uruguay", 2501, false, Confederation.CONMEBOL));
    }

    [TestMethod]
    public void CanGetTeamByName()
    {
        var team = new ProjectCopa.Team("Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);

        var found = _service.FindByName("Uruguay");

        Assert.AreEqual(team, found);
    }

    [TestMethod]
    public void GetTeamByNameThrowsIfNotFound()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _service.FindByName("NoExiste"));
    }

    [TestMethod]
    public void CanRemoveTeam()
    {
        var team = new ProjectCopa.Team("Uruguay", 2000, false, Confederation.CONMEBOL);

        _service.AddTeam(team);
        _service.RemoveTeam("Uruguay");

        Assert.AreEqual(0, _service.GetTeams().Count);
    }

    [TestMethod]
    public void CannotRemoveNonExistingTeam()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _service.RemoveTeam("NoExiste"));
    }

    [TestMethod]
    public void CannotAddTeamBeyondUefaQuota()
    {
        for (int i = 1; i <= 16; i++)
        {
            _service.AddTeam(new ProjectCopa.Team($"UEFA{i}", 1000 + i, false, Confederation.UEFA));
        }

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("UEFA17", 1100, false, Confederation.UEFA)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondConmebolQuota()
    {
        for (int i = 1; i <= 7; i++)
        {
            _service.AddTeam(new ProjectCopa.Team($"CONMEBOL{i}", 1400 + i, false, Confederation.CONMEBOL));
        }

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("CONMEBOL8", 1500, false, Confederation.CONMEBOL)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondConcacafQuota()
    {
        for (int i = 1; i <= 7; i++)
        {
            _service.AddTeam(new ProjectCopa.Team($"CONCACAF{i}", 1600 + i, false, Confederation.CONCACAF));
        }

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("CONCACAF8", 1700, false, Confederation.CONCACAF)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondCafQuota()
    {
        for (int i = 1; i <= 9; i++)
        {
            _service.AddTeam(new ProjectCopa.Team($"CAF{i}", 1800 + i, false, Confederation.CAF));
        }

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("CAF10", 1900, false, Confederation.CAF)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondAfcQuota()
    {
        for (int i = 1; i <= 8; i++)
        {
            _service.AddTeam(new ProjectCopa.Team($"AFC{i}", 2000 + i, false, Confederation.AFC));
        }

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("AFC9", 2100, false, Confederation.AFC)));
    }

    [TestMethod]
    public void CannotAddTeamBeyondOfcQuota()
    {
        _service.AddTeam(new ProjectCopa.Team("OFC1", 2200, false, Confederation.OFC));

        Assert.ThrowsException<InvalidOperationException>(() => _service.AddTeam(new ProjectCopa.Team("OFC2", 2300, false, Confederation.OFC)));
    }

    [TestMethod]
    public void FixtureOrderingWithSameSeedProducesSameOrderWhenRankingHasTies()
    {
        List<Team> teams = new List<ProjectCopa.Team>();

        for (int i = 0; i < 48; i++)
        {
            int ranking = 2500 - (i / 4) * 10;
            var conf = (Confederation)(i % 6);

            teams.Add(new ProjectCopa.Team($"Team{i + 1}", ranking, false, conf));
        }

        List<string> firstOrder = _service.OrderForFixture(teams, 2026).Select(t => t.Name).ToList();
        List<string> secondOrder = _service.OrderForFixture(teams, 2026).Select(t => t.Name).ToList();

        Assert.AreEqual(48, firstOrder.Count);
        CollectionAssert.AreEqual(firstOrder, secondOrder);
    }
}
