using DataAccess.Interfaces;
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
    private Mock<ITeamRepository> _teamRepositoryMock = null!;
    private RankingService _rankingService = null!;

    [TestInitialize]
    public void Setup()
    {
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _rankingService = new RankingService(_auditLogServiceMock.Object, _teamRepositoryMock.Object);
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

    [TestMethod]
    public void RevertMatchResult_UndoesTheAppliedDeltaAndClearsFlag()
    {
        // H7: revertir un resultado ya aplicado devuelve a ambos equipos a su ranking previo.
        Team homeTeam = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        Team awayTeam = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        MatchGroup match = BuildGroupMatch(homeTeam, awayTeam);
        match.SetGoals(2, 0);

        _rankingService.ApplyMatchResult(match, MatchPhase.GroupStage);
        match.MarkRankingApplied();

        _rankingService.RevertMatchResult(match);

        Assert.AreEqual(1500, homeTeam.FifaRanking);
        Assert.AreEqual(1200, awayTeam.FifaRanking);
        Assert.IsFalse(match.RankingApplied);
    }

    [TestMethod]
    public void ReapplyMatchResult_AfterEditingResult_MatchesAFreshSingleApply()
    {
        // H7: editar un resultado ya publicado debe dejar el ranking como si el nuevo marcador
        // hubiera sido el unico aplicado (revertir el delta anterior y aplicar el nuevo).
        Team editedHome = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        Team editedAway = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        MatchGroup edited = BuildGroupMatch(editedHome, editedAway);

        edited.SetGoals(2, 0);
        _rankingService.ReapplyMatchResult(edited);   // primera carga (local gana)
        edited.SetGoals(0, 2);
        _rankingService.ReapplyMatchResult(edited);   // edicion (visitante gana)

        Team freshHome = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        Team freshAway = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        MatchGroup fresh = BuildGroupMatch(freshHome, freshAway);
        fresh.SetGoals(0, 2);
        _rankingService.ApplyMatchResultOnce(fresh);

        Assert.AreEqual(freshHome.FifaRanking, editedHome.FifaRanking);
        Assert.AreEqual(freshAway.FifaRanking, editedAway.FifaRanking);
    }

    [TestMethod]
    public void ReapplyMatchResult_SameResultTwice_IsIdempotent()
    {
        // Re-aplicar el mismo marcador no debe acumular efecto Elo.
        Team home = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        Team away = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        MatchGroup match = BuildGroupMatch(home, away);
        match.SetGoals(2, 0);

        _rankingService.ReapplyMatchResult(match);
        int homeAfterFirst = home.FifaRanking;
        int awayAfterFirst = away.FifaRanking;
        _rankingService.ReapplyMatchResult(match);

        Assert.AreEqual(homeAfterFirst, home.FifaRanking);
        Assert.AreEqual(awayAfterFirst, away.FifaRanking);
    }

    private static MatchGroup BuildGroupMatch(Team homeTeam, Team awayTeam)
    {
        Stadium stadium = new Stadium("Test Stadium", "City", null, 30000);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        return MatchGroup.CreateInstance(10, DateTime.Now, homeTeam, awayTeam, stadium, group, journey);
    }

    [TestMethod]
    public void ApplyMatchResult_HomeWins_PersistsBothTeamsWithUpdatedRanking()
    {
        // Arrange
        var homeTeam = new Team(1, "Home", 1500, false, Confederation.CONMEBOL);
        var awayTeam = new Team(2, "Away", 1200, false, Confederation.CONMEBOL);
        var stadium = new Stadium("Test Stadium", "City", null, 30000);

        var group = new Group("A");
        var journey = Journey.CreateInstance(1);
        var match = MatchGroup.CreateInstance(10, DateTime.Now, homeTeam, awayTeam, stadium, group, journey);

        match.SetGoals(2, 0); // Home wins

        // Act
        _rankingService.ApplyMatchResult(match, MatchPhase.GroupStage);

        // Assert: el ranking nuevo debe persistirse via el repositorio, no solo mutarse en memoria
        _teamRepositoryMock.Verify(r => r.Update(
            It.Is<Team>(t => t.Id == homeTeam.Id && t.FifaRanking == 1510)), Times.Once);

        _teamRepositoryMock.Verify(r => r.Update(
            It.Is<Team>(t => t.Id == awayTeam.Id && t.FifaRanking == 1190)), Times.Once);
    }
}