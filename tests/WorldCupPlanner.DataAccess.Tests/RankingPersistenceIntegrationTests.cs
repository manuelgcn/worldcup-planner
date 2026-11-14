using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;

namespace WorldCupPlanner.DataAccess.Tests;

// EF arma Team con su constructor, que exige un ranking entre 300 y 2500: si se guarda un valor fuera
// de rango, toda lectura posterior de ese equipo falla. Los dobles de Services no lo detectan.
[TestClass]
public sealed class RankingPersistenceIntegrationTests
{
    private const int MinFifaRanking = 300;
    private const int MaxFifaRanking = 2500;

    private SqliteDbContextFactory _factory = null!;
    private TeamRepository _teamRepository = null!;
    private RankingService _rankingService = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new SqliteDbContextFactory();
        _teamRepository = new TeamRepository(_factory);
        _rankingService = new RankingService(
            AuditLogService.CreateInstance(new AuditLogRepository(_factory)),
            _teamRepository);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void ReRegisteringAResult_NearTheUpperBound_NeverStoresAnOutOfRangeRanking()
    {
        // El local pierde (2495 -> 2480), gana (-> 2495) y despues la derrota se corrige a empate:
        // revertir solo esa derrota lo deja en 2510 antes de aplicar el empate.
        Team home = AddTeam(1, 2495);
        Team away = AddTeam(2, 2495);
        MatchGroup correctedMatch = PlayMatch(home, away, homeGoals: 0, awayGoals: 1);
        PlayMatch(home, away, homeGoals: 1, awayGoals: 0);

        correctedMatch.SetGoals(1, 1);
        _rankingService.ReapplyMatchResult(correctedMatch);

        List<Team> stored = new TeamRepository(_factory).GetAll().ToList();
        Assert.IsTrue(stored.All(team => team.FifaRanking is >= MinFifaRanking and <= MaxFifaRanking));
    }

    private Team AddTeam(int id, int fifaRanking)
    {
        Team team = new Team(id, $"Team-{id}", fifaRanking, false, Confederation.UEFA);
        _teamRepository.Add(team);
        return team;
    }

    private MatchGroup PlayMatch(Team home, Team away, int homeGoals, int awayGoals)
    {
        MatchGroup match = MatchGroup.CreateInstance(
            10,
            new DateTime(2026, 6, 1),
            home,
            away,
            new Stadium("Test Stadium", "City", null, 30000),
            new Group("A"),
            Journey.CreateInstance(1));
        match.SetGoals(homeGoals, awayGoals);
        _rankingService.ApplyMatchResultOnce(match);
        return match;
    }
}
