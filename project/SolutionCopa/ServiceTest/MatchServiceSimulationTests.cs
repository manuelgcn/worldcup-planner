using ProjectCopa;
using Services;
using Services.DTOs;
using Services.Exceptions;
using ServiceTest.TestDoubles;

namespace ServiceTest;

[TestClass]
public sealed class MatchServiceSimulationTests
{
    private MatchServiceTestContext _context = null!;
    private CreateMatchRequestDto _createMatchRequest = null!;
    private MatchSimulationEngine _expectedEngine = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _context = new MatchServiceTestContext();
        _createMatchRequest = _context.NewGroupMatchRequest(id: 1);
        _expectedEngine = new MatchSimulationEngine();
    }

    [TestMethod]
    public void SimulateMatch_UsesTournamentSettingsSeed()
    {
        // Arrange
        Match expectedMatch = CreateExpectedMatch();
        _expectedEngine.Simulate(expectedMatch, 789);
        _context.Service.CreateMatch(_createMatchRequest);

        // Act
        _context.Service.SimulateMatch(matchId: 1);
        MatchDto result = _context.Service.GetById(matchId: 1);

        // Assert
        Assert.AreEqual(expectedMatch.HomeGoals, result.HomeGoals);
        Assert.AreEqual(expectedMatch.AwayGoals, result.AwayGoals);
    }

    [TestMethod]
    public void SimulateMatch_UpdatesRepository()
    {
        // Arrange
        _context.Service.CreateMatch(_createMatchRequest);
        int initialUpdateCalls = _context.MatchRepository.UpdateCalls;

        // Act
        _context.Service.SimulateMatch(matchId: 1);

        // Assert
        Assert.AreEqual(initialUpdateCalls + 1, _context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void SimulateMatch_CommitsChanges()
    {
        // Arrange
        _context.Service.CreateMatch(_createMatchRequest);
        int initialCommitCalls = _context.UnitOfWork.CommitCalls;

        // Act
        _context.Service.SimulateMatch(matchId: 1);

        // Assert
        Assert.AreEqual(initialCommitCalls + 1, _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void SimulateMatch_WithNonexistentMatch_ThrowsException()
    {
        // Arrange

        // Act & Assert
        Assert.ThrowsException<MatchServicesException>(() => _context.Service.SimulateMatch(matchId: 999));
    }

    private static Match CreateExpectedMatch()
    {
        Team homeTeam = new Team("Home Team", 1500, false, Confederation.CONCACAF);
        Team awayTeam = new Team("Away Team", 1600, false, Confederation.UEFA);
        Stadium stadium = new Stadium("Main Stadium", "Main City", "None", 25000);
        Group group = new Group("A");
        Journey journey = new Journey(1);

        return MatchGroup.CreateInstance(
            1,
            new DateTime(2026, 6, 21),
            homeTeam,
            awayTeam,
            stadium,
            group,
            journey);
    }
}