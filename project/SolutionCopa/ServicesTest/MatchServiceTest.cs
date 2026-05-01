using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Services.DTOs;
using Services;
using ServicesTest.TestDoubles;

namespace ServicesTest;

[TestClass]
public class MatchServiceTest
{
    [TestMethod]
    public void CreateMatchForGroupReturnsDtoWithId()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        MatchDto dto = context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForGroupCallsCommit()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchForEliminationReturnsDtoWithId()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        MatchDto dto = context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(2, dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForEliminationCallsCommit()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchThrowsWhenIdAlreadyExists()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.ThrowsException<ArgumentException>(() => context.Service.CreateMatch(context.NewGroupMatchRequest(1)));
    }

    [TestMethod]
    public void SetGoalsCallsUpdate()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        context.Service.SetGoals(1, 2, 1);

        Assert.AreEqual(1, context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void GetByDateRangeThrowsWhenRangeIsInvalid()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        Assert.ThrowsException<ArgumentException>(() =>
            context.Service.GetByDateRange(new DateTime(2026, 6, 30), new DateTime(2026, 6, 1)));
    }
}