using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.DTOs;
using Services.Exceptions;
using ServicesTest.TestDoubles;

namespace ServicesTest;

[TestClass]
public class MatchServiceTest
{

    private MatchServiceTestContext _context = null!;
    [TestInitialize]
    public void Setup()
    {
        _context = new MatchServiceTestContext();
    }
    
    [TestMethod]
    public void CreateMatchForGroupReturnsDtoWithId()
    {
        MatchDto dto = _context.Service.CreateMatch(_context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForGroupCallsCommit()
    {
        _context.Service.CreateMatch(_context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchForEliminationReturnsDtoWithId()
    {
        _context.AddSourceEliminationMatches();

        MatchDto dto = _context.Service.CreateMatch(_context.NewEliminationMatchRequest(2));

        Assert.AreEqual(2, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForEliminationCallsCommit()
    {
        _context.AddSourceEliminationMatches();

        _context.Service.CreateMatch(_context.NewEliminationMatchRequest(2));

        Assert.AreEqual(1, _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchThrowsWhenIdAlreadyExists()
    {
        _context.Service.CreateMatch(_context.NewGroupMatchRequest(1));

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.CreateMatch(_context.NewGroupMatchRequest(1)));
    }

    [TestMethod]
    public void SetGoalsCallsUpdate()
    {
        _context.Service.CreateMatch(_context.NewGroupMatchRequest(1));

        _context.Service.SetGoals(1, 2, 1);

        Assert.AreEqual(1, _context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void GetByDateRangeThrowsWhenRangeIsInvalid()
    {
        Assert.ThrowsException<MatchServicesException>(() =>
            _context.Service.GetByDateRange(new DateTime(2026, 6, 30), new DateTime(2026, 6, 1)));
    }
}