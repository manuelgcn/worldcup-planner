using Services.DTOs;
using Services.Exceptions;
using ServiceTest.TestDoubles;

namespace ServiceTest;

[TestClass]
public sealed class MatchServiceSimulationTests
{
    private MatchServiceTestContext _context = null!;
    private CreateMatchRequestDto _createMatchRequest = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _context = new MatchServiceTestContext();
        _createMatchRequest = _context.NewGroupMatchRequest(id: 1);
    }

    [TestMethod]
    public void SimulateMatch_CallsSimulationEngine()
    {
        // Arrange
        _context.Service.CreateMatch(_createMatchRequest);

        // Act
        _context.Service.SimulateMatch(matchId: 1);

        // Assert
        Assert.IsTrue(_context.SimulationEngine.SimulateWasCalled);
        Assert.IsNotNull(_context.SimulationEngine.LastMatchReceived);
    }

    [TestMethod]
    public void SimulateMatch_UsesTournamentSettingsSeed()
    {
        // Arrange
        _context.Service.CreateMatch(_createMatchRequest);

        // Act
        _context.Service.SimulateMatch(matchId: 1);

        // Assert
        Assert.AreEqual(789, _context.SimulationEngine.LastSeedReceived);
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
}
