using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Tests.TestDoubles;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class MatchServiceTest
{
    private MatchServiceTestContext _context = null!;

    [TestInitialize]
    public void TestSetup()
    {
        _context = new MatchServiceTestContext();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _context = null!;
    }

    [TestMethod]
    public void CreateGroupMatch_WithValidData_ReturnsMatchDtoWithCorrectId()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);

        // Act
        MatchDto result = _context.Service.CreateMatch(request);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(1, result.MatchId);
    }

    [TestMethod]
    public void CreateGroupMatch_WithValidData_LogsAuditEntry()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);

        _context.Service.CreateMatch(request);

        Assert.AreEqual(1, _context.AuditLogs.Count);
        Assert.AreEqual("CREATE_MATCH", _context.AuditLogs.First().Action);
    }

    [TestMethod]
    public void UpdateMatch_WithStadiumChange_LogsAuditEntry()
    {
        CreateMatchRequestDto createRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(createRequest);
        UpdateMatchRequestDto updateRequest = new UpdateMatchRequestDto { StadiumId = 30 };

        _context.Service.UpdateMatch(matchId: 1, request: updateRequest);

        Assert.IsTrue(_context.AuditLogs.Any(log => log.Details.Contains("Match updated")));
    }

    [TestMethod]
    public void CreateGroupMatch_WithChampionshipConfiguration_Succeeds()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(
            matchId: 1,
            championshipConfigurationId: 60);

        // Act
        MatchDto result = _context.Service.CreateMatch(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.MatchId);
    }

    [TestMethod]
    public void CreateEliminationMatch_WithValidData_ReturnsMatchDtoWithCorrectId()
    {
        // Arrange
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(matchId: 2);

        // Act
        MatchDto result = _context.Service.CreateMatch(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.MatchId);
    }

    [TestMethod]
    public void CreateEliminationMatch_WithoutChampionshipConfiguration_Succeeds()
    {
        // Arrange
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(
            matchId: 2,
            championshipConfigurationId: null);

        // Act
        MatchDto result = _context.Service.CreateMatch(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.MatchId);
    }
    
    [TestMethod]
    public void CreateMatch_WithNullRequest_ThrowsException()
    {
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request: null!));
    }

    [TestMethod]
    public void CreateMatch_WithDuplicateId_ThrowsException()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);

        // Act & Assert
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateMatch_WithNonexistentHomeTeam_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(homeTeamId: 999);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateMatch_WithNonexistentAwayTeam_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(awayTeamId: 999);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateMatch_WithNonexistentStadium_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(stadiumId: 999);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateMatch_WithNonexistentChampionshipConfiguration_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(championshipConfigurationId: 999);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }
    
    [TestMethod]
    public void CreateGroupMatch_WithMissingGroupName_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(groupName: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateGroupMatch_WithNonexistentGroup_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(groupName: "NoExiste");
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateGroupMatch_WithMissingJourneyId_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(journeyId: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateGroupMatch_WithNonexistentJourney_ThrowsException()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(journeyId: 999);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithMissingPhase_ThrowsException()
    {
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(phase: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithMissingMatchCode_ThrowsException()
    {
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(matchCode: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithMissingHomeSourceId_ThrowsException()
    {
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(matchHomeId: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithMissingAwaySourceId_ThrowsException()
    {
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto request = BuildEliminationMatchRequest(matchAwayId: null);
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithNonexistentHomeSourceMatch_ThrowsException()
    {
        // Add only the away source match (ID 200)
        _context.MatchRepository.Add(BuildMockMatch(
            id: 200,
            homeTeam: new Team(1, "Team 1", 1000, false, Confederation.CONCACAF),
            awayTeam: new Team(2, "Team 2", 2000, false, Confederation.UEFA),
            stadium: new Stadium("Stadium 1", "City 1", "None", 25000)));

        CreateMatchRequestDto request = BuildEliminationMatchRequest();
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void CreateEliminationMatch_WithNonexistentAwaySourceMatch_ThrowsException()
    {
        // Add only the home source match (ID 100)
        _context.MatchRepository.Add(BuildMockMatch(
            id: 100,
            homeTeam: new Team(3, "Team 3", 350, false, Confederation.CONCACAF),
            awayTeam: new Team(4, "Team 4", 400, false, Confederation.UEFA),
            stadium: new Stadium("Stadium 2", "City 2", "None", 25000)));

        CreateMatchRequestDto request = BuildEliminationMatchRequest();
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.CreateMatch(request));
    }

    [TestMethod]
    public void UpdateMatch_WithNullRequest_ThrowsException()
    {
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.UpdateMatch(matchId: 1, request: null!));
    }

    [TestMethod]
    public void UpdateMatch_WithStadiumChange_AppliesChange()
    {
        // Arrange
        CreateMatchRequestDto createRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(createRequest);
        UpdateMatchRequestDto updateRequest = new UpdateMatchRequestDto { StadiumId = 30 };

        // Act
        MatchDto result = _context.Service.UpdateMatch(matchId: 1, request: updateRequest);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.MatchId);
    }

    [TestMethod]
    public void UpdateMatch_WithDateChange_AppliesChange()
    {
        // Arrange
        CreateMatchRequestDto createRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(createRequest);
        DateTime newDate = new DateTime(2026, 6, 22, 18, 0, 0);
        UpdateMatchRequestDto updateRequest = new UpdateMatchRequestDto { MatchDate = newDate };

        // Act
        MatchDto result = _context.Service.UpdateMatch(matchId: 1, request: updateRequest);

        // Assert
        Assert.AreEqual(newDate, result.MatchDate);
    }

    [TestMethod]
    public void UpdateMatch_WithoutChanges_StillUpdates()
    {
        // Arrange
        CreateMatchRequestDto createRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(createRequest);
        UpdateMatchRequestDto updateRequest = new UpdateMatchRequestDto();
        int initialUpdateCalls = _context.MatchRepository.UpdateCalls;

        // Act
        _context.Service.UpdateMatch(matchId: 1, request: updateRequest);

        // Assert
        Assert.AreEqual(initialUpdateCalls + 1, _context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void UpdateMatch_WithNonexistentMatch_ThrowsException()
    {
        UpdateMatchRequestDto request = new UpdateMatchRequestDto();
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.UpdateMatch(matchId: 999, request));
    }

    [TestMethod]
    public void UpdateMatch_WithNonexistentStadium_ThrowsException()
    {
        // Arrange
        CreateMatchRequestDto createRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(createRequest);
        UpdateMatchRequestDto updateRequest = new UpdateMatchRequestDto { StadiumId = 999 };

        // Act & Assert
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.UpdateMatch(matchId: 1, request: updateRequest));
    }

    [TestMethod]
    public void DeleteMatch_WithValidId_DeletesMatch()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);
        int initialDeleteCalls = _context.MatchRepository.DeleteCalls;

        // Act
        _context.Service.DeleteMatch(matchId: 1);

        // Assert
        Assert.AreEqual(initialDeleteCalls + 1, _context.MatchRepository.DeleteCalls);
        
        // Verify match is actually deleted
        MatchServicesException deleteException = Assert.ThrowsExactly<MatchServicesException>(
            () => _context.Service.GetById(matchId: 1));
        Assert.IsNotNull(deleteException);
    }

    [TestMethod]
    public void DeleteMatch_WithValidId_LogsAuditEntry()
    {
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);

        _context.Service.DeleteMatch(matchId: 1);

        Assert.IsTrue(_context.AuditLogs.Any(log => log.Action == "DELETE_MATCH"));
    }

    [TestMethod]
    public void DeleteMatch_WithNonexistentMatch_ThrowsException()
    {
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.DeleteMatch(matchId: 999));
    }

    [TestMethod]
    public void GetMatchById_WithExistingMatch_ReturnsMatch()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);

        // Act
        MatchDto result = _context.Service.GetById(matchId: 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.MatchId);
    }

    [TestMethod]
    public void GetMatchById_WithNonexistentMatch_ThrowsException()
    {
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.GetById(matchId: 999));
    }

    [TestMethod]
    public void GetMatchesByGroup_WithValidGroupName_ReturnsOnlyGroupMatches()
    {
        // Arrange
        CreateMatchRequestDto groupRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(groupRequest);
        
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto eliminationRequest = BuildEliminationMatchRequest(matchId: 2);
        _context.Service.CreateMatch(eliminationRequest);

        // Act
        List<MatchDto> result = _context.Service.GetByGroup(groupName: "A");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].MatchId);
    }

    [TestMethod]
    public void GetMatchesByGroup_WithGroupNameHavingNoMatches_ReturnsEmptyList()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);

        // Act
        List<MatchDto> result = _context.Service.GetByGroup(groupName: "Z");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetMatchesByGroup_WithWhitespaceGroupName_ThrowsException()
    {
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.GetByGroup(groupName: "  "));
    }

    [TestMethod]
    public void GetMatchesByJourney_WithValidJourneyNumber_ReturnsMatchesForJourney()
    {
        // Arrange
        CreateMatchRequestDto groupRequest = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(groupRequest);
        
        _context.AddSourceEliminationMatches();
        CreateMatchRequestDto eliminationRequest = BuildEliminationMatchRequest(matchId: 2);
        _context.Service.CreateMatch(eliminationRequest);

        // Act
        List<MatchDto> result = _context.Service.GetByJourney(journeyNumber: 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].MatchId);
    }

    [TestMethod]
    public void GetMatchesByJourney_WithJourneyHavingNoMatches_ReturnsEmptyList()
    {
        // Arrange
        CreateMatchRequestDto request = BuildGroupMatchRequest(matchId: 1);
        _context.Service.CreateMatch(request);

        // Act
        List<MatchDto> result = _context.Service.GetByJourney(journeyNumber: 999);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetMatchesByDateRange_WithValidRange_ReturnsOnlyMatchesInRange()
    {
        // Arrange
        DateTime earlierDate = new DateTime(year: 2026, month: 6, day: 10);
        DateTime laterDate = new DateTime(year: 2026, month: 7, day: 10);
        
        CreateMatchRequestDto request1 = BuildGroupMatchRequest(matchId: 1, matchDate: earlierDate);
        CreateMatchRequestDto request2 = BuildGroupMatchRequest(matchId: 2, matchDate: laterDate);
        
        _context.Service.CreateMatch(request1);
        _context.Service.CreateMatch(request2);

        DateTime rangeStart = new DateTime(year: 2026, month: 6, day: 1);
        DateTime rangeEnd = new DateTime(year: 2026, month: 6, day: 30);

        // Act
        List<MatchDto> result = _context.Service.GetByDateRange(from: rangeStart, to: rangeEnd);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].MatchId);
    }

    [TestMethod]
    public void GetMatchesByDateRange_WithInvertedRange_ThrowsException()
    {
        DateTime from = new DateTime(year: 2026, month: 6, day: 30);
        DateTime to = new DateTime(year: 2026, month: 6, day: 1);
        
        Assert.ThrowsExactly<MatchServicesException>(() => _context.Service.GetByDateRange(from, to));
    }

    private static CreateMatchRequestDto BuildGroupMatchRequest(
        int matchId = 1,
        DateTime? matchDate = null,
        int homeTeamId = 10,
        int awayTeamId = 20,
        int stadiumId = 30,
        int? championshipConfigurationId = null,
        string? groupName = "A",
        int? journeyId = 1)
    {
        return new CreateMatchRequestDto
        {
            MatchId = matchId,
            MatchDate = matchDate ?? new DateTime(year: 2026, month: 6, day: 21),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            StadiumId = stadiumId,
            TournamentSettingsId = championshipConfigurationId,
            GroupName = groupName,
            JourneyId = journeyId
        };
    }

    /// <summary>
    /// Builds a valid CreateMatchRequestDto for elimination stage matches with sensible defaults.
    /// </summary>
    private static CreateMatchRequestDto BuildEliminationMatchRequest(
        int matchId = 2,
        int homeTeamId = 10,
        int awayTeamId = 20,
        int stadiumId = 30,
        int? championshipConfigurationId = 60,
        Phase? phase = Phase.RoundOf16,
        string? matchCode = "A1",
        int? matchHomeId = 100,
        int? matchAwayId = 200)
    {
        return new CreateMatchRequestDto
        {
            MatchId = matchId,
            MatchDate = new DateTime(year: 2026, month: 6, day: 25),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            StadiumId = stadiumId,
            IsElimination = true,
            TournamentSettingsId = championshipConfigurationId,
            Phase = phase,
            MatchCode = matchCode,
            MatchHomeId = matchHomeId,
            MatchAwayId = matchAwayId
        };
    }

    private static MockMatch BuildMockMatch(
        int id,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        DateTime? matchDate = null)
    {
        return new MockMatch(
            id: id,
            matchDate: matchDate ?? new DateTime(year: 2026, month: 6, day: 20),
            homeTeam: homeTeam,
            awayTeam: awayTeam,
            stadium: stadium);
    }
}
