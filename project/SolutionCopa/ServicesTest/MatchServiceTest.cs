using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.DTOs;
using Services.Exceptions;
using ServicesTest.TestDoubles;
using ServicesTest.TestDoubles.Fakes_Stubs;
using ProjectCopa;

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

    private static CreateMatchRequestDto NewGroupRequest(
        int id = 1,
        DateTime? date = null,
        int homeTeamId = 10,
        int awayTeamId = 20,
        int stadiumId = 30,
        int? championshipConfigurationId = null,
        int? groupId = 40,
        int? journeyId = 50)
    {
        return new CreateMatchRequestDto
        {
            MatchId = id,
            MatchDate = date ?? new DateTime(year: 2026, month: 6, day: 21),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            StadiumId = stadiumId,
            ChampionshipConfigurationId = championshipConfigurationId,
            GroupId = groupId,
            JourneyId = journeyId
        };
    }

    private static CreateMatchRequestDto NewEliminationRequest(
        int id = 2,
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
            MatchId = id,
            MatchDate = new DateTime(year: 2026, month: 6, day: 25),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            StadiumId = stadiumId,
            IsElimination = true,
            ChampionshipConfigurationId = championshipConfigurationId,
            Phase = phase,
            MatchCode = matchCode,
            MatchHomeId = matchHomeId,
            MatchAwayId = matchAwayId
        };
    }

    [TestMethod]
    public void CreateMatchForGroupReturnsDtoWithId()
    {
        MatchDto dto = _context.Service.CreateMatch(request: NewGroupRequest(id: 1));
        Assert.AreEqual(expected: 1, actual: dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForGroupCallsCommit()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        Assert.AreEqual(expected: 1, actual: _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchForEliminationReturnsDtoWithId()
    {
        _context.AddSourceEliminationMatches();

        MatchDto dto = _context.Service.CreateMatch(request: NewEliminationRequest(id: 2));

        Assert.AreEqual(expected: 2, actual: dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForEliminationCallsCommit()
    {
        _context.AddSourceEliminationMatches();

        _context.Service.CreateMatch(request: NewEliminationRequest(id: 2));

        Assert.AreEqual(expected: 1, actual: _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void CreateMatchThrowsWhenIdAlreadyExists()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(id: 1)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenRequestIsNull()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: null!));
    }

    [TestMethod]
    public void CreateMatchForGroupWithChampionshipConfigurationReturnsDtoWithId()
    {
        MatchDto dto = _context.Service.CreateMatch(request: NewGroupRequest(id: 1, championshipConfigurationId: 60));

        Assert.AreEqual(expected: 1, actual: dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchForEliminationWithoutChampionshipReturnsDtoWithId()
    {
        _context.AddSourceEliminationMatches();

        MatchDto dto = _context.Service.CreateMatch(request: NewEliminationRequest(id: 2, championshipConfigurationId: null));

        Assert.AreEqual(expected: 2, actual: dto.MatchId);
    }

    [TestMethod]
    public void CreateMatchThrowsWhenHomeTeamDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(homeTeamId: 999)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenAwayTeamDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(awayTeamId: 999)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenStadiumDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(stadiumId: 999)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenChampionshipConfigurationDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(championshipConfigurationId: 999)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenGroupIdIsMissing()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(groupId: null)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenGroupDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(groupId: 999)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenJourneyIdIsMissing()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(journeyId: null)));
    }

    [TestMethod]
    public void CreateMatchThrowsWhenJourneyDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewGroupRequest(journeyId: 999)));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenPhaseIsMissing()
    {
        _context.AddSourceEliminationMatches();

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest(phase: null)));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenMatchCodeIsMissing()
    {
        _context.AddSourceEliminationMatches();

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest(matchCode: null)));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenHomeSourceIdIsMissing()
    {
        _context.AddSourceEliminationMatches();

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest(matchHomeId: null)));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenAwaySourceIdIsMissing()
    {
        _context.AddSourceEliminationMatches();

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest(matchAwayId: null)));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenHomeSourceMatchDoesNotExist()
    {
        _context.MatchRepository.Add(match: new StubMatch(id: 200, matchDate: new DateTime(year: 2026, month: 6, day: 20), homeTeam: new StubTeam(), awayTeam: new StubTeam(), stadium: new StubStadium()));

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest()));
    }

    [TestMethod]
    public void CreateEliminationThrowsWhenAwaySourceMatchDoesNotExist()
    {
        _context.MatchRepository.Add(match: new StubMatch(id: 100, matchDate: new DateTime(year: 2026, month: 6, day: 20), homeTeam: new StubTeam(), awayTeam: new StubTeam(), stadium: new StubStadium()));

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.CreateMatch(request: NewEliminationRequest()));
    }

    [TestMethod]
    public void UpdateMatchThrowsWhenRequestIsNull()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.UpdateMatch(matchId: 1, request: null!));
    }

    [TestMethod]
    public void UpdateMatchChangesStadiumWhenProvided()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        var dto = _context.Service.UpdateMatch(matchId: 1, request: new UpdateMatchRequestDto { StadiumId = 30 });

        Assert.AreEqual(expected: 1, actual: dto.MatchId);
    }

    [TestMethod]
    public void UpdateMatchWithoutStadiumStillUpdatesAndCommits()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.UpdateMatch(matchId: 1, request: new UpdateMatchRequestDto());

        Assert.AreEqual(expected: 1, actual: _context.MatchRepository.UpdateCalls);
        Assert.AreEqual(expected: 2, actual: _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void UpdateMatchThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.UpdateMatch(matchId: 999, request: new UpdateMatchRequestDto()));
    }

    [TestMethod]
    public void UpdateMatchThrowsWhenStadiumDoesNotExist()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.UpdateMatch(matchId: 1, request: new UpdateMatchRequestDto { StadiumId = 999 }));
    }

    [TestMethod]
    public void DeleteMatchDeletesEntityAndCallsCommit()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.DeleteMatch(matchId: 1);

        Assert.AreEqual(expected: 1, actual: _context.MatchRepository.DeleteCalls);
        Assert.AreEqual(expected: 2, actual: _context.UnitOfWork.CommitCalls);
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.GetById(matchId: 1));
    }

    [TestMethod]
    public void DeleteMatchThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.DeleteMatch(matchId: 999));
    }

    [TestMethod]
    public void GetByIdReturnsStoredMatch()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        var dto = _context.Service.GetById(matchId: 1);

        Assert.AreEqual(expected: 1, actual: dto.MatchId);
    }

    [TestMethod]
    public void GetByIdThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.GetById(matchId: 999));
    }

    [TestMethod]
    public void GetByGroupReturnsOnlyGroupMatchesForGivenName()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));
        _context.AddSourceEliminationMatches();
        _context.Service.CreateMatch(request: NewEliminationRequest(id: 2));

        var result = _context.Service.GetByGroup(groupName: "A");

        Assert.AreEqual(expected: 1, actual: result.Count);
        Assert.AreEqual(expected: 1, actual: result[index: 0].MatchId);
    }

    [TestMethod]
    public void GetByGroupThrowsWhenNameIsWhitespace()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.GetByGroup(groupName: "  "));
    }

    [TestMethod]
    public void GetByJourneyReturnsMatchesForJourneyNumber()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));
        _context.AddSourceEliminationMatches();
        _context.Service.CreateMatch(request: NewEliminationRequest(id: 2));

        var result = _context.Service.GetByJourney(journeyNumber: 1);

        Assert.AreEqual(expected: 1, actual: result.Count);
        Assert.AreEqual(expected: 1, actual: result[index: 0].MatchId);
    }

    [TestMethod]
    public void GetByDateRangeReturnsOnlyMatchesInsideRange()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1, date: new DateTime(year: 2026, month: 6, day: 10)));
        _context.Service.CreateMatch(request: NewGroupRequest(id: 2, date: new DateTime(year: 2026, month: 7, day: 10)));

        var result = _context.Service.GetByDateRange(from: new DateTime(year: 2026, month: 6, day: 1), to: new DateTime(year: 2026, month: 6, day: 30));

        Assert.AreEqual(expected: 1, actual: result.Count);
        Assert.AreEqual(expected: 1, actual: result[index: 0].MatchId);
    }

    [TestMethod]
    public void SetGoalsCallsUpdate()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.SetGoals(matchId: 1, homeGoals: 2, awayGoals: 1);

        Assert.AreEqual(expected: 1, actual: _context.MatchRepository.UpdateCalls);
    }

    [TestMethod]
    public void SetGoalsThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.SetGoals(matchId: 999, homeGoals: 1, awayGoals: 1));
    }

    [TestMethod]
    public void AddGoalAddsHomeGoalWhenIsHomeTeamIsTrue()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.AddGoal(matchId: 1, isHomeTeam: true);

        var dto = _context.Service.GetById(matchId: 1);
        Assert.AreEqual(expected: 1, actual: dto.HomeGoals);
        Assert.AreEqual(expected: 0, actual: dto.AwayGoals);
    }

    [TestMethod]
    public void AddGoalAddsAwayGoalWhenIsHomeTeamIsFalse()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.AddGoal(matchId: 1, isHomeTeam: false);

        var dto = _context.Service.GetById(matchId: 1);
        Assert.AreEqual(expected: 0, actual: dto.HomeGoals);
        Assert.AreEqual(expected: 1, actual: dto.AwayGoals);
    }

    [TestMethod]
    public void AddGoalThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.AddGoal(matchId: 999, isHomeTeam: true));
    }

    [TestMethod]
    public void RemoveGoalRemovesHomeGoalWhenIsHomeTeamIsTrue()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));
        _context.Service.AddGoal(matchId: 1, isHomeTeam: true);

        _context.Service.RemoveGoal(matchId: 1, isHomeTeam: true);

        var dto = _context.Service.GetById(matchId: 1);
        Assert.AreEqual(expected: 0, actual: dto.HomeGoals);
    }

    [TestMethod]
    public void RemoveGoalRemovesAwayGoalWhenIsHomeTeamIsFalse()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));
        _context.Service.AddGoal(matchId: 1, isHomeTeam: false);

        _context.Service.RemoveGoal(matchId: 1, isHomeTeam: false);

        var dto = _context.Service.GetById(matchId: 1);
        Assert.AreEqual(expected: 0, actual: dto.AwayGoals);
    }

    [TestMethod]
    public void RemoveGoalThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.RemoveGoal(matchId: 999, isHomeTeam: true));
    }

    [TestMethod]
    public void PublishResultCallsUpdateAndCommit()
    {
        _context.Service.CreateMatch(request: NewGroupRequest(id: 1));

        _context.Service.PublishResult(matchId: 1);

        Assert.AreEqual(expected: 1, actual: _context.MatchRepository.UpdateCalls);
        Assert.AreEqual(expected: 2, actual: _context.UnitOfWork.CommitCalls);
    }

    [TestMethod]
    public void PublishResultThrowsWhenMatchDoesNotExist()
    {
        Assert.ThrowsException<MatchServicesException>(action: () => _context.Service.PublishResult(matchId: 999));
    }

    [TestMethod]
    public void GetByDateRangeThrowsWhenRangeIsInvalid()
    {
        Assert.ThrowsException<MatchServicesException>(action: () =>
            _context.Service.GetByDateRange(from: new DateTime(year: 2026, month: 6, day: 30), to: new DateTime(year: 2026, month: 6, day: 1)));
    }
}
