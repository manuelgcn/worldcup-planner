using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Tests;

// Las consultas angostas (COUNT / MAX / proyeccion) deben devolver lo mismo que la cuenta en memoria
// sobre GetAll() que reemplazan. Van sobre SQLite: InMemory no traduce a SQL y esconderia una consulta
// que el motor real no sabe resolver.
[TestClass]
public class NarrowQueryTests
{
    private const int PlayedGroupMatchId = 10;
    private const int LastGroupMatchId = 11;
    private const int GroupMatchIdAboveTheBracket = 5000;
    private const int FirstRoundOf32Id = 2000;
    private const int SecondRoundOf32Id = 2001;
    private const int RoundOf16Id = 2002;
    private const int HighestEliminationId = 2003;

    private static readonly DateTime LastGroupMatchDate = new DateTime(2026, 6, 5, 14, 0, 0);
    private static readonly TournamentSettings Settings =
        new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 0, 0, 0);

    private SqliteDbContextFactory _factory = null!;
    private MatchRepository _matches = null!;
    private EliminationMatchRepository _eliminations = null!;
    private TeamRepository _teams = null!;
    private StadiumRepository _stadiums = null!;
    private UserRepository _users = null!;

    private Team _a = null!;
    private Team _b = null!;
    private Team _c = null!;
    private Team _d = null!;
    private Stadium _stadium = null!;
    private MatchGroup _playedGroupMatch = null!;
    private MatchGroup _lastGroupMatch = null!;
    private MatchElimination _firstRoundOf32 = null!;
    private MatchElimination _secondRoundOf32 = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new SqliteDbContextFactory();
        _matches = new MatchRepository(_factory);
        _eliminations = new EliminationMatchRepository(_factory);
        _teams = new TeamRepository(_factory);
        _stadiums = new StadiumRepository(_factory);
        _users = new UserRepository(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void CountCountsGroupAndEliminationMatches()
    {
        Assert.AreEqual(0, _matches.Count());

        SeedGroupStage();
        SeedBracket();

        Assert.AreEqual(7, _matches.Count());
        Assert.AreEqual(_matches.GetAll().Count, _matches.Count());
    }

    [TestMethod]
    public void CountPlayedCountsOnlyMatchesWithAResultInBothMatchTypes()
    {
        Assert.AreEqual(0, _matches.CountPlayed());

        SeedGroupStage();
        SeedBracket();

        // Un partido de grupos con goles cargados, un 16avos cargado a mano y otro simulado.
        Assert.AreEqual(3, _matches.CountPlayed());
        Assert.AreEqual(_matches.GetAll().Count(match => match.HasResult), _matches.CountPlayed());
    }

    [TestMethod]
    public void LatestGroupMatchDateIgnoresLaterEliminationMatches()
    {
        Assert.IsNull(_matches.LatestGroupMatchDate());

        SeedGroupStage();
        SeedBracket();

        Assert.AreEqual(LastGroupMatchDate, _matches.LatestGroupMatchDate());
    }

    [TestMethod]
    public void MaxMatchIdIgnoresGroupMatchesWithAHigherId()
    {
        Assert.IsNull(_eliminations.MaxMatchId());

        SeedGroupStage();
        Assert.IsNull(_eliminations.MaxMatchId(), "Group matches are not part of the bracket.");

        SeedBracket();

        Assert.AreEqual(HighestEliminationId, _eliminations.MaxMatchId());
        Assert.AreEqual(_eliminations.GetAll().Max(match => match.MatchId), _eliminations.MaxMatchId());
    }

    [TestMethod]
    public void GetSourceMatchIdsReturnsTheMatchesThatFeedALaterRound()
    {
        Assert.AreEqual(0, _eliminations.GetSourceMatchIds().Count);

        SeedGroupStage();
        SeedRoundOf32();
        Assert.AreEqual(0, _eliminations.GetSourceMatchIds().Count, "The first round has no source matches.");

        SeedLaterRounds();

        int[] expected = { PlayedGroupMatchId, LastGroupMatchId, FirstRoundOf32Id, SecondRoundOf32Id };
        CollectionAssert.AreEquivalent(expected, _eliminations.GetSourceMatchIds().ToArray());
        CollectionAssert.AreEquivalent(
            _eliminations.GetAll()
                .OfType<MatchElimination>()
                .SelectMany(next => new[] { next.MatchHome?.MatchId, next.MatchAway?.MatchId })
                .OfType<int>()
                .Distinct()
                .ToArray(),
            _eliminations.GetSourceMatchIds().ToArray());
    }

    [TestMethod]
    public void TeamAndStadiumCountsMatchTheStoredRows()
    {
        Assert.AreEqual(0, _teams.Count());
        Assert.AreEqual(0, _stadiums.Count());

        SeedGroupStage();

        Assert.AreEqual(4, _teams.Count());
        Assert.AreEqual(_teams.GetAll().Count, _teams.Count());
        Assert.AreEqual(1, _stadiums.Count());
        Assert.AreEqual(_stadiums.GetAll().Count, _stadiums.Count());
    }

    [TestMethod]
    public void GetByRoleReturnsOnlyTheUsersWithThatRoleWithAllTheirRoles()
    {
        Assert.AreEqual(0, _users.GetByRole(UserRoles.Periodista).Count);

        _users.Add(MakeUser("periodista@mail.com", UserRoles.Periodista));
        _users.Add(MakeUser("admin@mail.com", UserRoles.Admin));
        _users.Add(MakeUser("ambos@mail.com", UserRoles.Editor, UserRoles.Periodista));

        IReadOnlyCollection<User> journalists = _users.GetByRole(UserRoles.Periodista);

        CollectionAssert.AreEquivalent(
            new[] { "periodista@mail.com", "ambos@mail.com" },
            journalists.Select(user => user.email).ToArray());
        Assert.IsTrue(journalists.Single(user => user.email == "ambos@mail.com").HasRole(UserRoles.Editor));
        CollectionAssert.AreEquivalent(
            _users.GetAll().Where(user => user.HasRole(UserRoles.Periodista)).Select(user => user.email).ToArray(),
            journalists.Select(user => user.email).ToArray());
    }

    private static User MakeUser(string email, params UserRoles[] roles)
    {
        User user = new User(0, "Juan", "Perez", email, "Pass123@", new DateTime(1990, 5, 15));
        foreach (UserRoles role in roles)
        {
            user.AddRole(role);
        }
        return user;
    }

    // Tres partidos de grupo: uno jugado, el ultimo por fecha y uno con un id mayor que todo el cuadro.
    private void SeedGroupStage()
    {
        _a = EfFixtureData.MakeTeam(1, "A");
        _b = EfFixtureData.MakeTeam(2, "B");
        _c = EfFixtureData.MakeTeam(3, "C");
        _d = EfFixtureData.MakeTeam(4, "D");
        _stadium = EfFixtureData.MakeStadium(1, "Estadio");
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);

        _playedGroupMatch = EfFixtureData.MakeGroupMatch(PlayedGroupMatchId, new DateTime(2026, 6, 1, 14, 0, 0), _a, _b, _stadium, group, journey);
        _playedGroupMatch.SetGoals(2, 1);
        _lastGroupMatch = EfFixtureData.MakeGroupMatch(LastGroupMatchId, LastGroupMatchDate, _c, _d, _stadium, group, journey);
        MatchGroup aboveTheBracket = EfFixtureData.MakeGroupMatch(GroupMatchIdAboveTheBracket, new DateTime(2026, 6, 3, 14, 0, 0), _a, _c, _stadium, group, journey);

        EfFixtureData.Seed(_factory, _a, _b, _c, _d, _stadium, group, journey, _playedGroupMatch, _lastGroupMatch, aboveTheBracket);
    }

    private void SeedBracket()
    {
        SeedRoundOf32();
        SeedLaterRounds();
    }

    // Primera ronda: sin partidos fuente. Uno con resultado cargado a mano y otro simulado.
    private void SeedRoundOf32()
    {
        _firstRoundOf32 = MatchElimination.CreateInitialRound(
            FirstRoundOf32Id, new DateTime(2026, 6, 20, 14, 0, 0), _a, _b, _stadium, Settings, Phase.RoundOf32, "A1");
        _firstRoundOf32.SetGoals(1, 0);
        _secondRoundOf32 = MatchElimination.CreateInitialRound(
            SecondRoundOf32Id, new DateTime(2026, 6, 21, 14, 0, 0), _c, _d, _stadium, Settings, Phase.RoundOf32, "A2");
        _secondRoundOf32.SetGoals(0, 2);
        _secondRoundOf32.MarkAsSimulated();

        _eliminations.Add(_firstRoundOf32);
        _eliminations.Add(_secondRoundOf32);
    }

    // Dos partidos sin jugar y posteriores a la fase de grupos: uno alimentado por los 16avos y otro por partidos de grupo.
    private void SeedLaterRounds()
    {
        _eliminations.Add(MatchElimination.CreateMatchWithConfig(
            RoundOf16Id, new DateTime(2026, 7, 1, 14, 0, 0), _a, _d, _stadium, Settings, Phase.RoundOf16, "C1", _firstRoundOf32, _secondRoundOf32));
        _eliminations.Add(EfFixtureData.MakeEliminationMatch(
            HighestEliminationId, new DateTime(2026, 7, 2, 14, 0, 0), _b, _c, _stadium, Phase.RoundOf16, _playedGroupMatch, _lastGroupMatch));
    }
}
