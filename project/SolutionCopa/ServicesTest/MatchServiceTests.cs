using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServicesTest.TestDoubles;

namespace ServicesTest;

[TestClass]
public class MatchServiceTests
{
    [TestMethod]
    public void CreateMatchForGroupReturnsDtoWithId()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        Services.DTOs.MatchDto dto = context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, dto.MatchId);
    }

    [TestMethod]
    public void Should_CreateGroupMatch_CallUnitOfWorkCommit()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void Should_CreateEliminationMatch_ReturnsMatchDtoWithId()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        Services.DTOs.MatchDto dto = context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(2, dto.MatchId);
    }

    [TestMethod]
    public void Should_CreateEliminationMatch_CallUnitOfWorkCommit()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.AddSourceEliminationMatches();

        context.Service.CreateMatch(context.NewEliminationMatchRequest(2));

        Assert.AreEqual(1, context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void Should_ThrowWhenCreatingMatch_WithDuplicateId()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        Assert.ThrowsException<ArgumentException>(() => context.Service.CreateMatch(context.NewGroupMatchRequest(1)));
    }

    [TestMethod]
    public void Should_CallRepositoryUpdate_When_SetGoals()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();
        context.Service.CreateMatch(context.NewGroupMatchRequest(1));

        context.Service.SetGoals(1, 2, 1);

        Assert.AreEqual(1, context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void GetByDateRange_ShouldThrow_When_FromIsAfterTo()
    {
        MatchServiceTestContext context = new MatchServiceTestContext();

        Assert.ThrowsException<ArgumentException>(() =>
            context.Service.GetByDateRange(new DateTime(2026, 6, 30), new DateTime(2026, 6, 1)));
    }
}
