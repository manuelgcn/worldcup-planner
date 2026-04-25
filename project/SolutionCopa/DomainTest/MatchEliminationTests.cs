using Moq;
using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public class MatchEliminationTests
{
    private static readonly DateTime MatchDate = new(2026, 6, 21);

    private Mock<ITeam> _homeTeam = null!;
    private Mock<ITeam> _awayTeam = null!;
    private Mock<IStadium> _stadium = null!;

    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
    }

    [TestMethod]
    public void MatchEliminationRejectsSameSourceMatchForHomeAndAway()
    {
        var sourceMatch = CreateSourceMatch(100);

        Assert.ThrowsException<MatchException>(() =>
            new MatchElimination(
                1,
                MatchDate,
                _homeTeam.Object,
                _awayTeam.Object,
                _stadium.Object,
                "QF1",
                sourceMatch,
                sourceMatch));
    }

    private static Match CreateSourceMatch(int matchId)
    {
        var sourceHomeTeam = new Mock<ITeam>();
        var sourceAwayTeam = new Mock<ITeam>();
        var sourceStadium = new Mock<IStadium>();

        return new Match(matchId, MatchDate, sourceHomeTeam.Object, sourceAwayTeam.Object, sourceStadium.Object);
    }
}
