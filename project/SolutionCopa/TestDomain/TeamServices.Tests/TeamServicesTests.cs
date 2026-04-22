using ProjectCopa;
using TeamServicesType = Services.TeamServices;

namespace TeamServices.Tests;

[TestClass]
public sealed class TeamServicesTests
{
    private const int RankingBaseUefa = 1000;
    private const int RankingBaseConmebol = 1400;
    private const int RankingBaseConcacaf = 1600;
    private const int RankingBaseCaf = 1800;
    private const int RankingBaseAfc = 2000;
    private const int RankingBaseOfc = 2200;

    private static readonly Confederation[] Confederations =
    [
        Confederation.AFC,
        Confederation.CAF,
        Confederation.CONCACAF,
        Confederation.CONMEBOL,
        Confederation.OFC,
        Confederation.UEFA
    ];

    [TestMethod]
    public void RegisteringTeamWithDuplicateNameThrowsInvalidOperationException()
    {
        var service = CreateService();
        var firstTeam = CreateTeam("Argentina", 2400, Confederation.CONMEBOL);
        var duplicateTeam = CreateTeam("Argentina", 2300, Confederation.CONMEBOL);

        service.AgregarEquipo(firstTeam);

        Assert.ThrowsException<InvalidOperationException>(() => service.AgregarEquipo(duplicateTeam));
    }

    [TestMethod]
    public void RegisteringTeamBeyondUefaQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.UEFA, quota: 16, RankingBaseUefa, "UEFA Team 17", 1200);
    }

    [TestMethod]
    public void RegisteringTeamBeyondConmebolQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.CONMEBOL, quota: 7, RankingBaseConmebol, "CONMEBOL Team 8", 1500);
    }

    [TestMethod]
    public void RegisteringTeamBeyondConcacafQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.CONCACAF, quota: 7, RankingBaseConcacaf, "CONCACAF Team 8", 1700);
    }

    [TestMethod]
    public void RegisteringTeamBeyondCafQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.CAF, quota: 9, RankingBaseCaf, "CAF Team 10", 1900);
    }

    [TestMethod]
    public void RegisteringTeamBeyondAfcQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.AFC, quota: 8, RankingBaseAfc, "AFC Team 9", 2100);
    }

    [TestMethod]
    public void RegisteringTeamBeyondOfcQuotaThrowsInvalidOperationException()
    {
        AssertQuotaExceeded(Confederation.OFC, quota: 1, RankingBaseOfc, "OFC Team 2", 2300);
    }

    [TestMethod]
    public void FixtureOrderingWithSameSeedProducesSameOrderWhenRankingHasTies()
    {
        var service = CreateService();
        var teams = Create48TeamsWithRankingTies();
        const int fixtureSeed = 2026;

        var firstOrder = service.OrdenarParaFixture(teams, fixtureSeed).Select(team => team.Nombre).ToList();
        var secondOrder = service.OrdenarParaFixture(teams, fixtureSeed).Select(team => team.Nombre).ToList();

        Assert.AreEqual(48, firstOrder.Count);
        CollectionAssert.AreEqual(firstOrder, secondOrder);
    }


    private static IReadOnlyCollection<Team> Create48TeamsWithRankingTies()
    {
        var teams = new List<Team>(capacity: 48);

        for (var i = 0; i < 48; i++)
        {
            var rankingWithTie = 2500 - (i / 4) * 10;
            var confederation = Confederations[i % Confederations.Length];
            teams.Add(CreateTeam($"Team {i + 1}", rankingWithTie, confederation));
        }

        return teams;
    }

    [TestMethod]
    public void FindingTeamByNameReturnsRegisteredTeam()
    {
        var service = CreateService();
        var argentina = CreateTeam("Argentina", 2400, Confederation.CONMEBOL);
        service.AgregarEquipo(argentina);

        var foundTeam = service.BuscarPorNombre("Argentina");

        Assert.IsNotNull(foundTeam);
        Assert.AreEqual("Argentina", foundTeam.Nombre);
    }

    [TestMethod]
    public void FindingTeamByNameWhenTeamDoesNotExistThrowsInvalidOperationException()
    {
        var service = CreateService();

        Assert.ThrowsException<InvalidOperationException>(() => service.BuscarPorNombre("Brazil"));
    }

    [TestMethod]
    public void DeletingRegisteredTeamRemovesItFromService()
    {
        var service = CreateService();
        var argentina = CreateTeam("Argentina", 2400, Confederation.CONMEBOL);
        service.AgregarEquipo(argentina);

        service.EliminarEquipo("Argentina");

        Assert.ThrowsException<InvalidOperationException>(() => service.BuscarPorNombre("Argentina"));
    }

    [TestMethod]
    public void DeletingNonExistingTeamThrowsInvalidOperationExceptionWithExpectedMessage()
    {
        var service = CreateService();

        var exception = Assert.ThrowsException<InvalidOperationException>(() => service.EliminarEquipo("Brazil"));

        Assert.AreEqual("Cannot delete team because it does not exist.", exception.Message);
    }

    private static TeamServicesType CreateService()
    {
        return new TeamServicesType();
    }

    private static Team CreateTeam(string name, int fifaRanking, Confederation confederation)
    {
        return new Team(name, fifaRanking, false, confederation);
    }

    private static void LoadTeams(TeamServicesType service, int count, Confederation confederation, int baseRanking)
    {
        for (var i = 1; i <= count; i++)
        {
            service.AgregarEquipo(CreateTeam($"{confederation} Team {i}", baseRanking + i, confederation));
        }
    }

    private static void AssertQuotaExceeded(
        Confederation confederation,
        int quota,
        int baseRanking,
        string exceededName,
        int exceededRanking)
    {
        var service = CreateService();

        LoadTeams(service, quota, confederation, baseRanking);

        Assert.ThrowsException<InvalidOperationException>(() =>
            service.AgregarEquipo(CreateTeam(exceededName, exceededRanking, confederation)));
    }
}




