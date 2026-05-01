using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServicesTest.TestDoubles;

namespace ServicesTest;

[TestClass]
public class MatchServiceTests
{
    [TestMethod]
    public void CreateMatchForGroupReturnsDtoWithId()
    {
        var context = new MatchServiceTestContext();

        var dto = context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForGroupCallsCommit()
    {
        var context = new MatchServiceTestContext();

        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchForEliminationReturnsDtoWithId()
    {
        var context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        var dto = context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(2, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForEliminationCallsCommit()
    {
        var context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchThrowsWhenIdAlreadyExists()
    {
        var context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.ThrowsException<ArgumentException>(() => context.Service.CreateMatch(context.NewGroupMatchRequest(1)));
    }

    [TestMethod]
    public void SetGoalsCallsUpdate()
    {
        var context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        context.Service.SetGoals(1, 2, 1);

        Assert.AreEqual(1, context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void GetByDateRangeThrowsWhenRangeIsInvalid()
    {
        var context = new MatchServiceTestContext();

        Assert.ThrowsException<ArgumentException>(() =>
            context.Service.GetByDateRange(new DateTime(2026, 6, 30), new DateTime(2026, 6, 1)));
    }
}
