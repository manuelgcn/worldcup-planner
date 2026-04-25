using ProjectCopa;
using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;
using Moq;
using Match = ProjectCopa.Match;

namespace DomainTest;

[TestClass]
public class MatchTests
{

    private Mock<ITeam> _homeTeam = null!;
    private Mock<ITeam> _awayTeam = null!;
    private Mock<IStadium> _stadium = null!;
    private Match _match = null!;

    private static readonly DateTime MatchDate = new DateTime(2022, 11, 21);

    private Match CreateMatchWithData()
    {
        return new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object);
    }
    
    [TestInitialize]
    public void Setup()
    {
        _homeTeam = new Mock<ITeam>();
        _awayTeam = new Mock<ITeam>();
        _stadium = new Mock<IStadium>();
        _match = new Match();
    }

    [TestMethod]
    public void MatchCanBeCreatedWithoutData()
    {
        Assert.IsNotNull(_match);
    }

    [TestMethod]
    public void MatchCanGetMatchId()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(1, _match.MatchId);
    }

    [TestMethod]
    public void MatchCanGetMatchDate()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(MatchDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchCanGetHomeTeam()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_homeTeam.Object, _match.HomeTeam);
    }

    [TestMethod]
    public void MatchCanGetAwayTeam()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_awayTeam.Object, _match.AwayTeam);
    }

    [TestMethod]
    public void MatchCanGetHomeGoals()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(0, _match.HomeGoals);
    }

    [TestMethod]
    public void MatchCanGetAwayGoals()
    {
        _match = CreateMatchWithData();
        Assert.AreEqual(0, _match.AwayGoals);
    }

    [TestMethod]
    public void MatchCanGetStadium()
    {
        _match = CreateMatchWithData();
        Assert.AreSame(_stadium.Object, _match.Stadium);
    }
    
    [TestMethod]
    public void MatchRejectsSameHomeAndAwayTeam()
    {
        var sameTeam = new Mock<ITeam>();
        Assert.ThrowsException<MatchException>(() =>
            new Match(1, MatchDate, sameTeam.Object, sameTeam.Object, _stadium.Object));
    }

    [TestMethod]
    public void MatchRejectsInvalidDate()
    {
        var configurationMock = new Mock<IChampConf>();
        configurationMock.Setup(config => config.StartDate()).Returns(MatchDate);
        var invalidDate = MatchDate.AddDays(-1);

        Assert.ThrowsException<MatchException>(() =>
            new Match(1, invalidDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, configurationMock.Object));
    }

    [TestMethod]
    public void MatchRejectsNullChampionshipConfiguration()
    {
        IChampConf? championshipConfiguration = null;

        Assert.ThrowsException<MatchException>(() => new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, championshipConfiguration!));
    }

    [TestMethod]
    public void MatchCanBeCreatedWithChampionshipConfiguration()
    {
        var championshipConfiguration = new Mock<IChampConf>();
        championshipConfiguration.Setup(config => config.StartDate()).Returns(MatchDate);

        _match = new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, _stadium.Object, championshipConfiguration.Object);

        championshipConfiguration.Verify(config => config.StartDate(), Times.Once);
        Assert.AreEqual(MatchDate, _match.MatchDate);
    }

    [TestMethod]
    public void MatchRejectsNullStadium()
    {
        Assert.ThrowsException<MatchException>(() =>
            new Match(1, MatchDate, _homeTeam.Object, _awayTeam.Object, null!));
    }

    [TestMethod]
    public void MatchCanChangeStadium()
    {
        _match = CreateMatchWithData();
        var newStadium = new Mock<IStadium>();

        _match.ChangeStadium(newStadium.Object);

        Assert.AreSame(newStadium.Object, _match.Stadium);
    } }

/*
 * GENERALIZATION TESTS NEEDED FOR MATCHGROUP AND MATCHELIMINATION:
 * 
 * MatchGroup Tests (Group Stage Matches):
 * ========================================
 * 1. MatchGroupCanBeCreatedWithGroupAndJourney - Validates creation with IGroup and IJourney
 * 2. MatchGroupRejectsNullGroup - Ensures IGroup is required
 * 3. MatchGroupRejectsNullJourney - Ensures IJourney is required
 * 4. MatchGroupCanGetGroup - Validates IGroup property getter
 * 5. MatchGroupCanGetJourney - Validates IJourney property getter
 * 6. MatchGroupCanBeCreatedWithChampionshipConfigurationAndGroupAndJourney - Validates overload with IChampConf
 * 7. MatchGroupInheritsMatchBehavior - Validates that MatchGroup inherits all Match domain rules (teams different, valid date, required stadium)
 * 8. MatchGroupCanChangeStadiumInheritedBehavior - Validates ChangeStadium(IStadium) works from base Match class
 * 
 * MatchElimination Tests (Knockout Stage Matches):
 * ================================================
 * 1. MatchEliminationCanBeCreatedWithParentMatches - Validates creation with IMatch references for home and away finalists
 * 2. MatchEliminationRejectsNullMatchCodeIdentifier - Ensures match code is required (e.g., "SF1", "F")
 * 3. MatchEliminationRejectsNullHomeParentMatch - Ensures home parent match (IMatch) is required
 * 4. MatchEliminationRejectsNullAwayParentMatch - Ensures away parent match (IMatch) is required
 * 5. MatchEliminationCanGetMatchCode - Validates match code property getter
 * 6. MatchEliminationCanGetHomeParentMatch - Validates home finalist match reference
 * 7. MatchEliminationCanGetAwayParentMatch - Validates away finalist match reference
 * 8. MatchEliminationCanBeCreatedWithChampionshipConfiguration - Validates overload with IChampConf
 * 9. MatchEliminationInheritsMatchBehavior - Validates that MatchElimination inherits all Match domain rules
 * 10. MatchEliminationCanChangeStadiumInheritedBehavior - Validates ChangeStadium behavior works from base
 * 
 * Integration Tests (Future):
 * ===========================
 * 1. CompetitionBracketCanTransitionMatchesFromGroupToEliminationStage - Winners from MatchGroup become home/away for MatchElimination
 * 2. VenueCanHostMultipleMatchesAcrossStages - Stadium can be used for both group and elimination matches
 * 3. MatchSequenceCanBeFollowedAcrossStages - Journey progression and match code sequencing are logical
 */
