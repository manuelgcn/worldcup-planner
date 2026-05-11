using DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class MatchRepositoryTests
{
    private MatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new MatchRepository();
    }

    [TestMethod]
    public void AddStoresMatchGroupAndExistsByIdReturnsTrue()
    {
        MatchGroup matchGroup = CreateGroupMatch(matchId: 1, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 1));

        _repository.Add(matchGroup);

        bool exists = _repository.ExistsById(matchId: 1);
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void AddStoresMatchEliminationAndGetByIdReturnsSameType()
    {
        MatchElimination matchElimination = CreateEliminationMatch(matchId: 10, date: new DateTime(2026, 6, 5));

        _repository.Add(matchElimination);

        Match? storedMatch = _repository.GetById(matchId: 10);
        Assert.IsNotNull(storedMatch);
        Assert.IsInstanceOfType<MatchElimination>(storedMatch);
    }

    [TestMethod]
    public void GetByIdReturnsNullWhenMatchDoesNotExist()
    {
        Match? storedMatch = _repository.GetById(matchId: 999);

        Assert.IsNull(storedMatch);
    }

    [TestMethod]
    public void UpdateReplacesExistingMatchWithSameId()
    {
        MatchGroup originalMatch = CreateGroupMatch(matchId: 3, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 2));
        MatchGroup updatedMatch = CreateGroupMatch(matchId: 3, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 10));
        _repository.Add(originalMatch);

        _repository.Update(updatedMatch);

        Match? storedMatch = _repository.GetById(matchId: 3);
        Assert.IsNotNull(storedMatch);
        Assert.AreEqual(expected: new DateTime(2026, 6, 10), actual: storedMatch.MatchDate);
    }

    [TestMethod]
    public void UpdateAddsMatchWhenIdDoesNotExist()
    {
        MatchGroup match = CreateGroupMatch(matchId: 4, groupName: "B", journeyNumber: 2, date: new DateTime(2026, 6, 11));

        _repository.Update(match);

        bool exists = _repository.ExistsById(matchId: 4);
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void DeleteRemovesStoredMatch()
    {
        MatchGroup match = CreateGroupMatch(matchId: 5, groupName: "C", journeyNumber: 1, date: new DateTime(2026, 6, 3));
        _repository.Add(match);

        _repository.Delete(match);

        Match? storedMatch = _repository.GetById(matchId: 5);
        Assert.IsNull(storedMatch);
    }

    [TestMethod]
    public void DeleteDoesNothingWhenMatchIsNotPresent()
    {
        MatchGroup existingMatch = CreateGroupMatch(matchId: 6, groupName: "C", journeyNumber: 1, date: new DateTime(2026, 6, 4));
        MatchGroup nonStoredMatch = CreateGroupMatch(matchId: 99, groupName: "C", journeyNumber: 2, date: new DateTime(2026, 6, 20));
        _repository.Add(existingMatch);

        _repository.Delete(nonStoredMatch);

        Match? storedMatch = _repository.GetById(matchId: 6);
        Assert.IsNotNull(storedMatch);
    }

    [TestMethod]
    public void GetByGroupReturnsOnlyMatchGroupFromRequestedGroup()
    {
        MatchGroup groupAMatch = CreateGroupMatch(matchId: 7, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 5));
        MatchGroup groupBMatch = CreateGroupMatch(matchId: 8, groupName: "B", journeyNumber: 1, date: new DateTime(2026, 6, 5));
        MatchElimination eliminationMatch = CreateEliminationMatch(matchId: 11, date: new DateTime(2026, 6, 6));
        _repository.Add(groupAMatch);
        _repository.Add(groupBMatch);
        _repository.Add(eliminationMatch);

        List<Match> result = _repository.GetByGroup(groupName: "A");

        Assert.AreEqual(expected: 1, actual: result.Count);
        Assert.AreEqual(expected: 7, actual: result[0].MatchId);
    }

    [TestMethod]
    public void GetByGroupReturnsEmptyListWhenNoGroupMatches()
    {
        MatchElimination eliminationMatch = CreateEliminationMatch(matchId: 12, date: new DateTime(2026, 6, 7));
        _repository.Add(eliminationMatch);

        List<Match> result = _repository.GetByGroup(groupName: "Z");

        Assert.AreEqual(expected: 0, actual: result.Count);
    }

    [TestMethod]
    public void GetByJourneyReturnsOnlyMatchGroupFromRequestedJourney()
    {
        MatchGroup journeyOne = CreateGroupMatch(matchId: 13, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 8));
        MatchGroup journeyTwo = CreateGroupMatch(matchId: 14, groupName: "A", journeyNumber: 2, date: new DateTime(2026, 6, 9));
        MatchElimination eliminationMatch = CreateEliminationMatch(matchId: 15, date: new DateTime(2026, 6, 9));
        _repository.Add(journeyOne);
        _repository.Add(journeyTwo);
        _repository.Add(eliminationMatch);

        List<Match> result = _repository.GetByJourney(journeyNumber: 2);

        Assert.AreEqual(expected: 1, actual: result.Count);
        Assert.AreEqual(expected: 14, actual: result[0].MatchId);
    }

    [TestMethod]
    public void GetByJourneyReturnsEmptyListWhenNoJourneyMatches()
    {
        MatchElimination eliminationMatch = CreateEliminationMatch(matchId: 16, date: new DateTime(2026, 6, 10));
        _repository.Add(eliminationMatch);

        List<Match> result = _repository.GetByJourney(journeyNumber: 3);

        Assert.AreEqual(expected: 0, actual: result.Count);
    }

    [TestMethod]
    public void GetByDateRangeReturnsMatchesInsideInclusiveRange()
    {
        MatchGroup boundaryStart = CreateGroupMatch(matchId: 17, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 6, 10));
        MatchElimination boundaryEnd = CreateEliminationMatch(matchId: 18, date: new DateTime(2026, 6, 20));
        MatchGroup outOfRange = CreateGroupMatch(matchId: 19, groupName: "A", journeyNumber: 2, date: new DateTime(2026, 6, 25));
        _repository.Add(boundaryStart);
        _repository.Add(boundaryEnd);
        _repository.Add(outOfRange);

        List<Match> result = _repository.GetByDateRange(from: new DateTime(2026, 6, 10), to: new DateTime(2026, 6, 20));

        Assert.AreEqual(expected: 2, actual: result.Count);
        Assert.IsTrue(result.Any(match => match.MatchId == 17));
        Assert.IsTrue(result.Any(match => match.MatchId == 18));
    }

    [TestMethod]
    public void GetByDateRangeReturnsEmptyListWhenNothingMatches()
    {
        MatchGroup match = CreateGroupMatch(matchId: 20, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 7, 1));
        _repository.Add(match);

        List<Match> result = _repository.GetByDateRange(from: new DateTime(2026, 6, 1), to: new DateTime(2026, 6, 30));

        Assert.AreEqual(expected: 0, actual: result.Count);
    }

    private static MatchGroup CreateGroupMatch(int matchId, string groupName, int journeyNumber, DateTime date)
    {
        Team homeTeam = new Team(matchId + 1, name: $"Team-{matchId}-H", fifaRanking: 1000 + matchId, isFictitious: true, confederation: Confederation.CONMEBOL);
        Team awayTeam = new Team(matchId + 2, name: $"Team-{matchId}-A", fifaRanking: 900 + matchId, isFictitious: true, confederation: Confederation.CONMEBOL);
        Stadium stadium = new Stadium(name: $"Stadium-{matchId}", city: "Test City", description: null, capacity: 50000);
        int groupId = groupName[0] - 'A' + 1;
        Group group = new Group(id: groupId, name: groupName);
        Journey journey = Journey.CreateInstance(number: journeyNumber);

        return MatchGroup.CreateInstance(matchId, date, homeTeam, awayTeam, stadium, group, journey);
    }

    private static MatchElimination CreateEliminationMatch(int matchId, DateTime date)
    {
        MatchGroup sourceHome = CreateGroupMatch(matchId: matchId + 100, groupName: "X", journeyNumber: 1, date: date.AddDays(-1));
        MatchGroup sourceAway = CreateGroupMatch(matchId: matchId + 200, groupName: "Y", journeyNumber: 1, date: date.AddDays(-1));
        Team homeTeam = new Team(matchId + 1, name: $"Elim-Team-{matchId}-H", fifaRanking: 1200 + matchId, isFictitious: true, confederation: Confederation.CONMEBOL);
        Team awayTeam = new Team(matchId + 2, name: $"Elim-Team-{matchId}-A", fifaRanking: 1100 + matchId, isFictitious: true, confederation: Confederation.CONMEBOL);
        Stadium stadium = new Stadium(name: $"Elim-Stadium-{matchId}", city: "Test City", description: null, capacity: 60000);

        return MatchElimination.Create(
            id: matchId,
            matchDate: date,
            homeTeam: homeTeam,
            awayTeam: awayTeam,
            stadium: stadium,
            phase: Phase.RoundOf16,
            matchCode: $"E{matchId}",
            matchHome: sourceHome,
            matchAway: sourceAway);
    }
}
