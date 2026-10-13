using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Exceptions;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class MatchServiceIncidentTests
{
    private const int GroupMatchId = 1;
    private const int HomeTeamId = 10;
    private const int AwayTeamId = 20;
    private const int StadiumId = 30;
    private const string GroupName = "A";
    private const int JourneyId = 1;
    private const int TournamentSettingsId = 1;
    private const int ForeignTeamId = 999;
    private const int UnknownIncidentId = 999;
    private const string EditorEmail = "editor@worldcup.com";

    private MatchServiceTestContext _context = null!;

    [TestInitialize]
    public void TestSetup()
    {
        _context = new MatchServiceTestContext();
        SeedGroupMatch();
    }

    [TestMethod]
    public void AddYellowCard_AddsIncidentToMatch()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);

        Match match = GetGroupMatch();
        Assert.AreEqual(1, match.CountYellowCards(match.HomeTeam!));
        Assert.AreEqual(0, match.CountRedCards(match.HomeTeam!));
    }

    [TestMethod]
    public void AddRedCard_AddsIncidentToMatch()
    {
        _context.Service.AddRedCard(GroupMatchId, AwayTeamId, minute: 75, EditorEmail);

        Match match = GetGroupMatch();
        Assert.AreEqual(1, match.CountRedCards(match.AwayTeam!));
        Assert.AreEqual(0, match.CountYellowCards(match.AwayTeam!));
    }

    [TestMethod]
    public void AddYellowCard_RegistersAuditWithPerformedBy()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);

        AuditLog? log = _context.AuditLogs.FirstOrDefault(l => l.Action == AuditActions.IncidentAdded);
        Assert.IsNotNull(log);
        Assert.AreEqual(EditorEmail, log.PerformedByEmail);
        Assert.AreEqual(GroupMatchId, log.AffectedEntityId);
    }

    [TestMethod]
    public void AddYellowCard_OnTeamNotInMatch_Throws()
    {
        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.AddYellowCard(GroupMatchId, ForeignTeamId, minute: 30, EditorEmail));
    }

    [TestMethod]
    public void AddYellowCard_WithNegativeMinute_Throws()
    {
        Assert.ThrowsException<MatchException>(
            () => _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: -1, EditorEmail));
    }

    [TestMethod]
    public void AddYellowCard_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail));
    }

    [TestMethod]
    public void AddRedCard_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.AddRedCard(GroupMatchId, AwayTeamId, minute: 75, EditorEmail));
    }

    [TestMethod]
    public void RemoveIncident_RemovesIncidentFromMatch()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);
        MatchIncidentDto incident = _context.Service.GetIncidents(GroupMatchId).Single();

        _context.Service.RemoveIncident(GroupMatchId, incident.Id, EditorEmail);

        Match match = GetGroupMatch();
        Assert.AreEqual(0, match.Incidents.Count);
    }

    [TestMethod]
    public void RemoveIncident_RegistersAuditWithPerformedBy()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);
        MatchIncidentDto incident = _context.Service.GetIncidents(GroupMatchId).Single();

        _context.Service.RemoveIncident(GroupMatchId, incident.Id, EditorEmail);

        AuditLog? log = _context.AuditLogs.FirstOrDefault(l => l.Action == AuditActions.IncidentRemoved);
        Assert.IsNotNull(log);
        Assert.AreEqual(EditorEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void RemoveIncident_WithUnknownIncidentId_Throws()
    {
        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.RemoveIncident(GroupMatchId, UnknownIncidentId, EditorEmail));
    }

    [TestMethod]
    public void RemoveIncident_OnGroupMatch_WhenKnockoutExists_Throws()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);
        MatchIncidentDto incident = _context.Service.GetIncidents(GroupMatchId).Single();
        _context.SeedFakeEliminationMatchInRepository();

        Assert.ThrowsException<MatchServicesException>(
            () => _context.Service.RemoveIncident(GroupMatchId, incident.Id, EditorEmail));
    }

    [TestMethod]
    public void GetIncidents_ReturnsIncidentDetails()
    {
        _context.Service.AddYellowCard(GroupMatchId, HomeTeamId, minute: 30, EditorEmail);
        _context.Service.AddRedCard(GroupMatchId, AwayTeamId, minute: 75, EditorEmail);

        List<MatchIncidentDto> incidents = _context.Service.GetIncidents(GroupMatchId);

        Assert.AreEqual(2, incidents.Count);
        MatchIncidentDto yellow = incidents.Single(i => i.Type == MatchIncidentDto.YellowCardType);
        Assert.AreEqual(30, yellow.Minute);
        Assert.AreEqual(HomeTeamId, yellow.TeamId);
        Assert.AreEqual("Home Team", yellow.TeamName);
        MatchIncidentDto red = incidents.Single(i => i.Type == MatchIncidentDto.RedCardType);
        Assert.AreEqual(75, red.Minute);
        Assert.AreEqual(AwayTeamId, red.TeamId);
    }

    [TestMethod]
    public void GetIncidents_WithUnknownMatch_Throws()
    {
        Assert.ThrowsException<MatchServicesException>(() => _context.Service.GetIncidents(matchId: 404));
    }

    private Match GetGroupMatch()
    {
        return _context.MatchRepository.GetById(GroupMatchId)!;
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
}
