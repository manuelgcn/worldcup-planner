using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class MatchRepositoryTests
{
    private InMemoryDbContextFactory _factory = null!;
    private MatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new InMemoryDbContextFactory();
        _repository = new MatchRepository(_factory);
    }

    [TestMethod]
    public void AddStoresMatchGroupAndExistsByIdReturnsTrue()
    {
        AddIsolatedGroupMatch(matchId: 1, groupName: "A", journeyNumber: 1, date: Date(1));

        Assert.IsTrue(_repository.ExistsById(1));
    }

    [TestMethod]
    public void AddStoresMatchEliminationAndGetByIdReturnsSameType()
    {
        AddBracket(eliminationId: 201, homeGoalsSource: 2, awayGoalsSource: 0);

        Match? stored = _repository.GetById(201);

        Assert.IsNotNull(stored);
        Assert.IsInstanceOfType<MatchElimination>(stored);
    }

    [TestMethod]
    public void GetByIdReturnsNullWhenMatchDoesNotExist()
    {
        Assert.IsNull(_repository.GetById(999));
    }

    [TestMethod]
    public void UpdatePersistsEditedResult()
    {
        AddIsolatedGroupMatch(matchId: 3, groupName: "A", journeyNumber: 1, date: Date(2));
        Match stored = _repository.GetById(3)!;          // detached (AsNoTracking)
        stored.SetGoals(2, 1);

        _repository.Update(stored);

        Match reloaded = _repository.GetById(3)!;
        Assert.AreEqual(2, reloaded.HomeGoals);
        Assert.AreEqual(1, reloaded.AwayGoals);
    }

    [TestMethod]
    public void UpdatePersistsEliminationPenaltyResult()
    {
        AddBracket(eliminationId: 201, homeGoalsSource: 1, awayGoalsSource: 1);
        MatchElimination stored = (MatchElimination)_repository.GetById(201)!;
        stored.SetGoals(1, 1);
        stored.SetPenaltyShootoutResult(5, 4);

        _repository.Update(stored);

        MatchElimination reloaded = (MatchElimination)_repository.GetById(201)!;
        Assert.IsTrue(reloaded.WonByPenalties);
        Assert.AreEqual(5, reloaded.HomePenaltyGoals);
        Assert.AreEqual(4, reloaded.AwayPenaltyGoals);
    }

    [TestMethod]
    public void UpdatePersistsStadiumChange()
    {
        Stadium first = EfFixtureData.MakeStadium(1, "First");
        Stadium second = EfFixtureData.MakeStadium(2, "Second");
        Team home = EfFixtureData.MakeTeam(11, "Home");
        Team away = EfFixtureData.MakeTeam(12, "Away");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        EfFixtureData.Seed(_factory, first, second, home, away, group, journey);
        _repository.Add(EfFixtureData.MakeGroupMatch(5, Date(1), home, away, first, group, journey));

        Match stored = _repository.GetById(5)!;
        stored.ChangeStadium(second);
        _repository.Update(stored);

        Assert.AreEqual(1, _repository.GetByStadium(2).Count);
        Assert.AreEqual(0, _repository.GetByStadium(1).Count);
    }

    [TestMethod]
    public void UpdateAddsMatchWhenIdDoesNotExist()
    {
        Team home = EfFixtureData.MakeTeam(41, "Home");
        Team away = EfFixtureData.MakeTeam(42, "Away");
        Stadium stadium = EfFixtureData.MakeStadium(43, "Stadium");
        Group group = new Group("B");
        Journey journey = Journey.CreateInstance(2);
        EfFixtureData.Seed(_factory, home, away, stadium, group, journey);

        _repository.Update(EfFixtureData.MakeGroupMatch(4, Date(11), home, away, stadium, group, journey));

        Assert.IsTrue(_repository.ExistsById(4));
    }

    [TestMethod]
    public void DeleteRemovesStoredMatch()
    {
        AddIsolatedGroupMatch(matchId: 5, groupName: "C", journeyNumber: 1, date: Date(3));

        _repository.Delete(_repository.GetById(5)!);

        Assert.IsNull(_repository.GetById(5));
    }

    [TestMethod]
    public void DeleteDoesNothingWhenMatchIsNotPresent()
    {
        AddIsolatedGroupMatch(matchId: 6, groupName: "C", journeyNumber: 1, date: Date(4));
        Match notStored = EfFixtureData.MakeGroupMatch(
            99, Date(20), EfFixtureData.MakeTeam(9901, "x"), EfFixtureData.MakeTeam(9902, "y"),
            EfFixtureData.MakeStadium(9903, "z"), new Group("C"), Journey.CreateInstance(2));

        _repository.Delete(notStored);

        Assert.IsNotNull(_repository.GetById(6));
    }

    [TestMethod]
    public void GetByGroupReturnsOnlyMatchesFromRequestedGroup()
    {
        Group groupA = new Group("A");
        Group groupB = new Group("B");
        Journey journey = Journey.CreateInstance(1);
        Stadium stadium = EfFixtureData.MakeStadium(1, "St1");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, groupA, groupB, journey, stadium, teams[0], teams[1], teams[2], teams[3]);

        _repository.Add(EfFixtureData.MakeGroupMatch(7, Date(5), teams[0], teams[1], stadium, groupA, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(8, Date(5), teams[2], teams[3], stadium, groupB, journey));

        List<Match> result = _repository.GetByGroup("A");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(7, result[0].MatchId);
    }

    [TestMethod]
    public void GetByJourneyReturnsOnlyMatchesFromRequestedJourney()
    {
        Group group = new Group("A");
        Journey journeyOne = Journey.CreateInstance(1);
        Journey journeyTwo = Journey.CreateInstance(2);
        Stadium stadium = EfFixtureData.MakeStadium(1, "St1");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, group, journeyOne, journeyTwo, stadium, teams[0], teams[1], teams[2], teams[3]);

        _repository.Add(EfFixtureData.MakeGroupMatch(13, Date(8), teams[0], teams[1], stadium, group, journeyOne));
        _repository.Add(EfFixtureData.MakeGroupMatch(14, Date(9), teams[2], teams[3], stadium, group, journeyTwo));

        List<Match> result = _repository.GetByJourney(2);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(14, result[0].MatchId);
    }

    [TestMethod]
    public void GetByStadiumReturnsOnlyMatchesAtThatStadium()
    {
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        Stadium centenario = EfFixtureData.MakeStadium(1, "Centenario");
        Stadium maracana = EfFixtureData.MakeStadium(2, "Maracana");
        Team[] teams = FourTeams(1);
        EfFixtureData.Seed(_factory, group, journey, centenario, maracana, teams[0], teams[1], teams[2], teams[3]);

        _repository.Add(EfFixtureData.MakeGroupMatch(1, Date(1), teams[0], teams[1], centenario, group, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(2, Date(2), teams[2], teams[3], maracana, group, journey));

        List<Match> atCentenario = _repository.GetByStadium(1);

        Assert.AreEqual(1, atCentenario.Count);
        Assert.AreEqual(1, atCentenario[0].MatchId);
    }

    [TestMethod]
    public void GetByPhaseReturnsOnlyEliminationMatchesOfThatPhase()
    {
        AddBracket(eliminationId: 10, homeGoalsSource: 2, awayGoalsSource: 0, phase: Phase.RoundOf16, sourceIdBase: 100, groupHomeName: "X", groupAwayName: "Y");
        AddBracket(eliminationId: 11, homeGoalsSource: 2, awayGoalsSource: 0, phase: Phase.QuarterFinals, sourceIdBase: 300, groupHomeName: "P", groupAwayName: "Q");

        List<Match> result = _repository.GetByPhase(Phase.RoundOf16);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(10, result[0].MatchId);
    }

    [TestMethod]
    public void GetByDateRangeReturnsMatchesInsideInclusiveRange()
    {
        AddIsolatedGroupMatch(matchId: 17, groupName: "A", journeyNumber: 1, date: Date(10));
        AddIsolatedGroupMatch(matchId: 18, groupName: "B", journeyNumber: 1, date: Date(20));
        AddIsolatedGroupMatch(matchId: 19, groupName: "C", journeyNumber: 2, date: Date(25));

        List<Match> result = _repository.GetByDateRange(Date(10), Date(20));

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(match => match.MatchId == 17));
        Assert.IsTrue(result.Any(match => match.MatchId == 18));
    }

    [TestMethod]
    public void GetByDateRangeReturnsEmptyListWhenNothingMatches()
    {
        AddIsolatedGroupMatch(matchId: 20, groupName: "A", journeyNumber: 1, date: new DateTime(2026, 7, 1));

        List<Match> result = _repository.GetByDateRange(Date(1), Date(30));

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredMatches()
    {
        AddIsolatedGroupMatch(matchId: 1, groupName: "A", journeyNumber: 1, date: Date(1));
        AddIsolatedGroupMatch(matchId: 2, groupName: "B", journeyNumber: 1, date: Date(2));

        Assert.AreEqual(2, _repository.GetAll().Count);
    }

    [TestMethod]
    public void AddTwoMatchesSharingRelatedDoesNotDuplicateRows()
    {
        // Trampa (a): equipos y estadio ya existen y se comparten entre dos partidos.
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        Stadium stadium = EfFixtureData.MakeStadium(1, "Shared");
        Team home = EfFixtureData.MakeTeam(1, "Home");
        Team away = EfFixtureData.MakeTeam(2, "Away");
        EfFixtureData.Seed(_factory, group, journey, stadium, home, away);

        _repository.Add(EfFixtureData.MakeGroupMatch(1, Date(1), home, away, stadium, group, journey));
        _repository.Add(EfFixtureData.MakeGroupMatch(2, Date(4), away, home, stadium, group, journey));

        Assert.AreEqual(2, EfFixtureData.Count<Match>(_factory));
        Assert.AreEqual(2, EfFixtureData.Count<Team>(_factory));      // sin equipos duplicados
        Assert.AreEqual(1, EfFixtureData.Count<Stadium>(_factory));   // sin estadios duplicados
    }

    [TestMethod]
    public void ClearRemovesAllStoredMatchesIncludingBracket()
    {
        // Trampa (c): el bracket tiene autorreferencias (MatchHome/MatchAway); Clear no debe explotar.
        AddIsolatedGroupMatch(matchId: 1, groupName: "A", journeyNumber: 1, date: Date(1));
        AddBracket(eliminationId: 201, homeGoalsSource: 2, awayGoalsSource: 0, sourceIdBase: 500);

        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
        Assert.AreEqual(0, EfFixtureData.Count<Match>(_factory));
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void GetAllResolvesBracketChainAcrossMatches()
    {
        // El partido de eliminacion referencia a sus fuentes; con identity resolution
        // MatchHome/MatchAway apuntan a las MISMAS instancias cargadas en el resultado,
        // por lo que GetWinner() resuelve a traves de la cadena.
        AddBracket(eliminationId: 201, homeGoalsSource: 3, awayGoalsSource: 1);

        MatchElimination elimination = _repository.GetAll().OfType<MatchElimination>().Single();

        Assert.IsNotNull(elimination.MatchHome);
        Team? sourceWinner = elimination.MatchHome!.GetWinner();
        Assert.IsNotNull(sourceWinner);
        Assert.AreEqual(elimination.HomeTeam!.Id, sourceWinner!.Id);   // el local salio del ganador de MatchHome
    }

    private static DateTime Date(int day) => new DateTime(2026, 6, day);

    private static Team[] FourTeams(int baseId) =>
    [
        EfFixtureData.MakeTeam(baseId, $"T{baseId}"),
        EfFixtureData.MakeTeam(baseId + 1, $"T{baseId + 1}"),
        EfFixtureData.MakeTeam(baseId + 2, $"T{baseId + 2}"),
        EfFixtureData.MakeTeam(baseId + 3, $"T{baseId + 3}"),
    ];

    // Siembra prerrequisitos propios (equipos/estadio/grupo/jornada) y agrega un partido de grupo aislado.
    private void AddIsolatedGroupMatch(int matchId, string groupName, int journeyNumber, DateTime date)
    {
        Team home = EfFixtureData.MakeTeam(matchId * 100 + 1, $"H{matchId}");
        Team away = EfFixtureData.MakeTeam(matchId * 100 + 2, $"A{matchId}");
        Stadium stadium = EfFixtureData.MakeStadium(matchId * 100 + 3, $"St{matchId}");
        Group group = new Group(groupName);
        Journey journey = Journey.CreateInstance(journeyNumber);
        EfFixtureData.Seed(_factory, home, away, stadium, group, journey);
        _repository.Add(EfFixtureData.MakeGroupMatch(matchId, date, home, away, stadium, group, journey));
    }

    // Siembra dos partidos de grupo (con resultado) como fuentes y agrega un partido de eliminacion
    // cuyo local es el ganador de MatchHome. Los ids de las fuentes se derivan de sourceIdBase para
    // poder armar varios brackets en un mismo test sin colisiones de PK.
    private void AddBracket(int eliminationId, int homeGoalsSource, int awayGoalsSource, Phase phase = Phase.RoundOf16,
        int sourceIdBase = 100, string groupHomeName = "X", string groupAwayName = "Y")
    {
        Team a = EfFixtureData.MakeTeam(sourceIdBase + 1, $"A{sourceIdBase}");
        Team b = EfFixtureData.MakeTeam(sourceIdBase + 2, $"B{sourceIdBase}");
        Team c = EfFixtureData.MakeTeam(sourceIdBase + 3, $"C{sourceIdBase}");
        Team d = EfFixtureData.MakeTeam(sourceIdBase + 4, $"D{sourceIdBase}");
        Stadium stadium = EfFixtureData.MakeStadium(sourceIdBase + 5, $"St{sourceIdBase}");
        Group groupX = new Group(groupHomeName);
        Group groupY = new Group(groupAwayName);
        Journey journey = Journey.CreateInstance(sourceIdBase);     // numero unico por bracket

        MatchGroup sourceHome = EfFixtureData.MakeGroupMatch(sourceIdBase + 10, Date(1), a, b, stadium, groupX, journey);
        sourceHome.SetGoals(homeGoalsSource, awayGoalsSource);
        MatchGroup sourceAway = EfFixtureData.MakeGroupMatch(sourceIdBase + 11, Date(1), c, d, stadium, groupY, journey);
        sourceAway.SetGoals(0, 2);

        EfFixtureData.Seed(_factory, a, b, c, d, stadium, groupX, groupY, journey, sourceHome, sourceAway);

        Team eliminationHome = sourceHome.GetWinner() ?? a;
        Team eliminationAway = sourceAway.GetWinner() ?? d;
        _repository.Add(EfFixtureData.MakeEliminationMatch(
            eliminationId, Date(15), eliminationHome, eliminationAway, stadium, phase, sourceHome, sourceAway));
    }
}
