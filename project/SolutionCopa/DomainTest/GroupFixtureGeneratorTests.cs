using ProjectCopa;
using ProjectCopa.Exceptions;

namespace DomainTest;

[TestClass]
public class GroupFixtureGeneratorTests
{
    private const int TotalGroups = 12;
    private const int TeamsPerGroup = 4;
    private const int ExpectedTotalMatches = 72;
    private const int BaseFifaRanking = 1000;
    private const int DefaultStadiumCapacity = 20000;
    private const int RequiredStadiumCount = 4;
    private const int GroupsToTriggerNextDaySchedule = 7;
    private const int SimultaneousJornadaNumber = 3;
    private const int ExpectedSimultaneousMatchesPerGroup = 2;
    private const int NextDayOffset = 1;
    private const int SingleGroupCount = 1;
    private const int MaxMatchesPerStadiumPerDay = 3;

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private static List<Group> CreateGroups(int totalGroups)
    {
        List<Group> groups = new List<Group>();
        int teamId = 1;
        for (int groupIndex = 0; groupIndex < totalGroups; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);
            for (int teamIndex = 0; teamIndex < TeamsPerGroup; teamIndex++)
            {
                Team team = new Team(teamId, string.Format("G{0}_T{1}", groupName, teamIndex + 1), BaseFifaRanking, false, Confederation.UEFA);
                teamId++;
                group.AddTeam(team);
            }
            groups.Add(group);
        }
        return groups;
    }

    private static List<Stadium> CreateStadiums(int count = RequiredStadiumCount)
    {
        List<Stadium> stadiums = new List<Stadium>();
        for (int index = 0; index < count; index++)
        {
            stadiums.Add(new Stadium(string.Format("Arena {0}", (char)('A' + index)), "City", string.Empty, DefaultStadiumCapacity));
        }
        return stadiums;
    }

    [TestMethod]
    public void GenerateMatches_CreatesSeventyTwoMatches_FullTournament()
    {
        List<Group> groups = CreateGroups(TotalGroups);
        List<Stadium> stadiums = CreateStadiums();
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        
        List<Match> matches = generator.GenerateMatches(groups, stadiums, DefaultStartDate);

        Assert.AreEqual(ExpectedTotalMatches, matches.Count);
    }

    [TestMethod]
    public void GenerateMatches_SchedulesSeventhGroupOnNextDay()
    {
        List<Group> groups = CreateGroups(GroupsToTriggerNextDaySchedule);
        List<Stadium> stadiums = CreateStadiums();
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        
        List<Match> matches = generator.GenerateMatches(groups, stadiums, DefaultStartDate);

        MatchGroup firstMatch = matches.OfType<MatchGroup>().First(match => match.Group!.Name == "A");
        MatchGroup seventhMatch = matches.OfType<MatchGroup>().First(match => match.Group!.Name == "G");

        Assert.AreEqual(DefaultStartDate.Date, firstMatch.MatchDate.Date);
        Assert.AreEqual(DefaultStartDate.AddDays(NextDayOffset).Date, seventhMatch.MatchDate.Date);
    }

    [TestMethod]
    public void GenerateMatches_JornadaThree_SimultaneousPerGroup()
    {
        List<Group> groups = CreateGroups(SingleGroupCount + 1);
        List<Stadium> stadiums = CreateStadiums();
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        
        List<Match> matches = generator.GenerateMatches(groups, stadiums, DefaultStartDate);

        foreach (Group group in groups)
        {
            List<MatchGroup> jornadaThreeMatches = matches.OfType<MatchGroup>()
                .Where(match => match.Group!.Name == group.Name && match.Journey!.Number == SimultaneousJornadaNumber)
                .ToList();

            Assert.AreEqual(ExpectedSimultaneousMatchesPerGroup, jornadaThreeMatches.Count);
            Assert.AreEqual(jornadaThreeMatches[0].MatchDate, jornadaThreeMatches[1].MatchDate);
            Assert.AreNotEqual(jornadaThreeMatches[0].Stadium!.Name, jornadaThreeMatches[1].Stadium!.Name);
        }
    }

    [TestMethod]
    public void GenerateMatches_RespectsAlphabeticalStadiumRotation()
    {
        List<Group> groups = CreateGroups(SingleGroupCount);
        List<Stadium> stadiums = new List<Stadium>
        {
            new Stadium("Z Arena", "City", string.Empty, DefaultStadiumCapacity),
            new Stadium("A Arena", "City", string.Empty, DefaultStadiumCapacity)
        };
        GroupFixtureGenerator generator = new GroupFixtureGenerator();

        List<Match> matches = generator.GenerateMatches(groups, stadiums, DefaultStartDate);

        Assert.AreEqual("A Arena", matches[0].Stadium!.Name);
    }

    [TestMethod]
    public void GenerateMatches_NullGroups_ThrowsArgumentNullException()
    {
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        List<Stadium> stadiums = CreateStadiums();

        Assert.ThrowsException<ArgumentNullException>(() =>
            generator.GenerateMatches(null!, stadiums, DefaultStartDate));
    }

    [TestMethod]
    public void GenerateMatches_NullStadiums_ThrowsArgumentNullException()
    {
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        List<Group> groups = CreateGroups(SingleGroupCount);

        Assert.ThrowsException<ArgumentNullException>(() =>
            generator.GenerateMatches(groups, null!, DefaultStartDate));
    }

    [TestMethod]
    public void GenerateMatches_IncompleteGroup_ThrowsMatchException()
    {
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        List<Group> groups = new List<Group> { new Group("Fail") };
        List<Stadium> stadiums = CreateStadiums();

        Assert.ThrowsException<MatchException>(() =>
            generator.GenerateMatches(groups, stadiums, DefaultStartDate));
    }

    [TestMethod]
    public void GenerateMatches_NoStadiums_ThrowsMatchException()
    {
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        List<Group> groups = CreateGroups(SingleGroupCount);
        List<Stadium> emptyStadiums = new List<Stadium>();

        Assert.ThrowsException<MatchException>(() =>
            generator.GenerateMatches(groups, emptyStadiums, DefaultStartDate));
    }

    [TestMethod]
    public void GenerateMatches_RespectsMaxMatchesPerStadiumPerDay()
    {
        List<Group> groups = CreateGroups(TotalGroups);
        List<Stadium> stadiums = CreateStadiums();
        GroupFixtureGenerator generator = new GroupFixtureGenerator();
        
        List<Match> matches = generator.GenerateMatches(groups, stadiums, DefaultStartDate);

        IEnumerable<IGrouping<string, Match>> dailyUsage = matches.GroupBy(match => match.Stadium!.Name + match.MatchDate.Date);

        foreach (IGrouping<string, Match> group in dailyUsage)
        {
            Assert.IsTrue(group.Count() <= MaxMatchesPerStadiumPerDay);
        }
    }
}