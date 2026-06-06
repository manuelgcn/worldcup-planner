using DataAccess.Exceptions;
using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

// Defensa en profundidad de MatchEf.Add: un partido de eliminacion cuyas fuentes (MatchHome/MatchAway)
// no esten persistidas debe fallar rapido con una excepcion clara de DataAccess -nombrando el MatchId
// ofensor- en lugar de marcar la fuente como existente y dejar que el INSERT salga con un FK colgante.
[TestClass]
public class MatchPersistenceGuardTests
{
    private InMemoryDbContextFactory _factory = null!;
    private EliminationMatchRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new InMemoryDbContextFactory();
        _repository = new EliminationMatchRepository(_factory);
    }

    [TestMethod]
    public void Add_WhenSourceMatchIsNotPersisted_ThrowsMatchPersistenceExceptionNamingTheOffender()
    {
        Team a = EfFixtureData.MakeTeam(1, "A");
        Team b = EfFixtureData.MakeTeam(2, "B");
        Team c = EfFixtureData.MakeTeam(3, "C");
        Team d = EfFixtureData.MakeTeam(4, "D");
        Stadium stadium = EfFixtureData.MakeStadium(5, "St");
        Journey journey = Journey.CreateInstance(1);
        // Fuentes NUNCA sembradas en la base.
        MatchGroup sourceHome = EfFixtureData.MakeGroupMatch(900, new DateTime(2026, 6, 1), a, b, stadium, new Group("X"), journey);
        MatchGroup sourceAway = EfFixtureData.MakeGroupMatch(901, new DateTime(2026, 6, 1), c, d, stadium, new Group("Y"), journey);
        MatchElimination elimination = EfFixtureData.MakeEliminationMatch(
            20, new DateTime(2026, 6, 15), a, d, stadium, Phase.RoundOf16, sourceHome, sourceAway);

        MatchPersistenceException exception = Assert.ThrowsException<MatchPersistenceException>(
            () => _repository.Add(elimination));

        Assert.IsTrue(
            exception.Message.Contains("900"),
            $"Message should name the offending source MatchId, got '{exception.Message}'");
        Assert.IsFalse(_repository.ExistsById(20), "The elimination match must not have been persisted.");
    }

    [TestMethod]
    public void Add_WhenSourceMatchesArePersisted_DoesNotThrow()
    {
        Team a = EfFixtureData.MakeTeam(1, "A");
        Team b = EfFixtureData.MakeTeam(2, "B");
        Team c = EfFixtureData.MakeTeam(3, "C");
        Team d = EfFixtureData.MakeTeam(4, "D");
        Stadium stadium = EfFixtureData.MakeStadium(5, "St");
        Group groupX = new Group("X");
        Group groupY = new Group("Y");
        Journey journey = Journey.CreateInstance(1);
        MatchGroup sourceHome = EfFixtureData.MakeGroupMatch(900, new DateTime(2026, 6, 1), a, b, stadium, groupX, journey);
        MatchGroup sourceAway = EfFixtureData.MakeGroupMatch(901, new DateTime(2026, 6, 1), c, d, stadium, groupY, journey);
        EfFixtureData.Seed(_factory, a, b, c, d, stadium, groupX, groupY, journey, sourceHome, sourceAway);

        _repository.Add(EfFixtureData.MakeEliminationMatch(
            20, new DateTime(2026, 6, 15), a, d, stadium, Phase.RoundOf16, sourceHome, sourceAway));

        Assert.IsTrue(_repository.ExistsById(20));
    }
}
