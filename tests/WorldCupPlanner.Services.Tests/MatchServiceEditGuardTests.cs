using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class MatchServiceEditGuardTests
{
    private const int GroupMatchId = 1;
    private const int HomeTeamId = 10;
    private const int AwayTeamId = 20;
    private const int StadiumId = 30;
    private const string GroupName = "A";
    private const int JourneyId = 1;
    private const int TournamentSettingsId = 1;

    private MatchServiceTestContext _context = null!;

    [TestInitialize]
    public void TestSetup()
    {
        _context = new MatchServiceTestContext();
        SeedGroupMatch();
    }

    [TestMethod]
    public void UpdateMatch_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.UpdateMatch(GroupMatchId, new UpdateMatchRequestDto { StadiumId = StadiumId }));
    }

    [TestMethod]
    public void UpdateMatch_OnGroupMatch_WhenKnockoutDoesNotExist_Succeeds()
    {
        MatchDto result = _context.Service.UpdateMatch(GroupMatchId, new UpdateMatchRequestDto { StadiumId = StadiumId });

        Assert.AreEqual(GroupMatchId, result.MatchId);
    }

    [TestMethod]
    public void SetGoals_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.SetGoals(GroupMatchId, 2, 1));
    }

    [TestMethod]
    public void AddGoal_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.AddGoal(GroupMatchId, isHomeTeam: true));
    }

    [TestMethod]
    public void RemoveGoal_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.Service.SetGoals(GroupMatchId, 2, 0);
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.RemoveGoal(GroupMatchId, isHomeTeam: true));
    }

    [TestMethod]
    public void SimulateMatch_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.SimulateMatch(GroupMatchId));
    }

    [TestMethod]
    public void DeleteMatch_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(() => _context.Service.DeleteMatch(GroupMatchId));
    }

    [TestMethod]
    public void UpdateMatch_OnEliminationMatch_WhenKnockoutExists_Succeeds()
    {
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto createRequest = BuildEliminationMatchRequest(matchId: 2);
        _context.Service.CreateMatch(createRequest);
        _context.SeedFakeEliminationMatchInRepository();

        MatchDto result = _context.Service.UpdateMatch(2, new UpdateMatchRequestDto { StadiumId = StadiumId });

        Assert.AreEqual(2, result.MatchId);
    }

    private void SeedGroupMatch()
    {
        CreateMatchRequestDto request = new CreateMatchRequestDto
        {
            MatchId = GroupMatchId,
            MatchDate = new DateTime(2026, 6, 1, 14, 0, 0),
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            StadiumId = StadiumId,
            IsElimination = false,
            TournamentSettingsId = TournamentSettingsId,
            GroupName = GroupName,
            JourneyId = JourneyId
        };
        _context.Service.CreateMatch(request);
    }

    private static CreateMatchRequestDto BuildEliminationMatchRequest(int matchId)
    {
        return new CreateMatchRequestDto
        {
            MatchId = matchId,
            MatchDate = new DateTime(2026, 7, 1, 14, 0, 0),
            HomeTeamId = HomeTeamId,
            AwayTeamId = AwayTeamId,
            StadiumId = StadiumId,
            IsElimination = true,
            TournamentSettingsId = TournamentSettingsId,
            Phase = Phase.RoundOf16,
            MatchCode = "A1",
            MatchHomeId = 100,
            MatchAwayId = 200
        };
    }
}
