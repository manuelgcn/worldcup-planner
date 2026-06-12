using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectCopa;
using Services;
using Services.Interfaces;

namespace ServiceTest;

[TestClass]
public class RankingServiceTests
{
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private RankingService _rankingService = null!;

    [TestInitialize]
    public void Setup()
    {
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _rankingService = new RankingService(_auditLogServiceMock.Object);
    }

    [TestMethod]
    public void ApplyMatchResult_HomeWins_UpdatesBothRankingsAndLogs()
    {
        // Arrange
        var homeTeam = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        var awayTeam = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        var stadium = new Stadium("Test Stadium", "City", null, 30000);
        
        // We use MatchGroup as a concrete Match
        var group = new Group("A");
        var journey = Journey.CreateInstance(1);
        var match = MatchGroup.CreateInstance(10, DateTime.Now, homeTeam, awayTeam, stadium, group, journey);

        match.SetGoals(2, 0); // Home wins

        // Act
        _rankingService.ApplyMatchResult(match, MatchPhase.GroupStage);

        // Assert
        Assert.AreEqual(1510, homeTeam.FifaRanking); // 1500 -> 1510 (based on formula)
        Assert.AreEqual(1190, awayTeam.FifaRanking); // 1200 -> 1190

        _auditLogServiceMock.Verify(a => a.Log(
            AuditActions.RankingUpdated,
            It.Is<string>(s => s.Contains("Home") && s.Contains("1500 -> 1510")),
            nameof(Team),
            homeTeam.Id), Times.Once);

        _auditLogServiceMock.Verify(a => a.Log(
            AuditActions.RankingUpdated,
            It.Is<string>(s => s.Contains("Away") && s.Contains("1200 -> 1190")),
            nameof(Team),
            awayTeam.Id), Times.Once);
    }
}