using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class DtoCoverageTests
{
    [TestMethod]
    public void MatchDtoCanStoreAndExposeAllProperties()
    {
        MatchDto dto = new MatchDto
        {
            MatchId = 7,
            MatchDate = new DateTime(2026, 6, 20),
            HomeTeamId = 1,
            AwayTeamId = 2,
            StadiumId = 3,
            HomeGoals = 4,
            AwayGoals = 5,
            IsElimination = true,
            GroupName = "A",
            JourneyNumber = 6,
            MatchCode = "R16-1",
            Phase = Phase.RoundOf16,
            MatchHomeId = 10,
            MatchAwayId = 11,
            TournamentSettingsId = 12
        };

        Assert.AreEqual(7, dto.MatchId);
        Assert.AreEqual(new DateTime(2026, 6, 20), dto.MatchDate);
        Assert.AreEqual(1, dto.HomeTeamId);
        Assert.AreEqual(2, dto.AwayTeamId);
        Assert.AreEqual(3, dto.StadiumId);
        Assert.AreEqual(4, dto.HomeGoals);
        Assert.AreEqual(5, dto.AwayGoals);
        Assert.IsTrue(dto.IsElimination);
        Assert.AreEqual("A", dto.GroupName);
        Assert.AreEqual(6, dto.JourneyNumber);
        Assert.AreEqual("R16-1", dto.MatchCode);
        Assert.AreEqual(Phase.RoundOf16, dto.Phase);
        Assert.AreEqual(10, dto.MatchHomeId);
        Assert.AreEqual(11, dto.MatchAwayId);
        Assert.AreEqual(12, dto.TournamentSettingsId);
    }

    [TestMethod]
    public void UpdateMatchRequestDtoCanStoreAndExposeAllProperties()
    {
        UpdateMatchRequestDto dto = new UpdateMatchRequestDto
        {
            MatchDate = new DateTime(2026, 6, 21),
            HomeTeamId = 20,
            AwayTeamId = 21,
            StadiumId = 22,
            TournamentSettingsId = 23,
            GroupName = "B",
            JourneyId = 24,
            Phase = Phase.QuarterFinals,
            MatchCode = "QF-1",
            MatchHomeId = 25,
            MatchAwayId = 26
        };

        Assert.AreEqual(new DateTime(2026, 6, 21), dto.MatchDate);
        Assert.AreEqual(20, dto.HomeTeamId);
        Assert.AreEqual(21, dto.AwayTeamId);
        Assert.AreEqual(22, dto.StadiumId);
        Assert.AreEqual(23, dto.TournamentSettingsId);
        Assert.AreEqual("B", dto.GroupName);
        Assert.AreEqual(24, dto.JourneyId);
        Assert.AreEqual(Phase.QuarterFinals, dto.Phase);
        Assert.AreEqual("QF-1", dto.MatchCode);
        Assert.AreEqual(25, dto.MatchHomeId);
        Assert.AreEqual(26, dto.MatchAwayId);
    }

    [TestMethod]
    public void ImportResultExposesImportedTeamsAndCount()
    {
        Team firstTeam = new Team(1, "One", 1500, false, Confederation.UEFA);
        Team secondTeam = new Team(2, "Two", 1499, false, Confederation.UEFA);
        IReadOnlyCollection<Team> teams = new List<Team> { firstTeam, secondTeam };

        ImportResult result = new ImportResult(teams);

        Assert.AreSame(teams, result.ImportedTeams);
        Assert.AreEqual(2, result.ImportedCount);
    }

    [TestMethod]
    public void MatchGroupDtoFactoryBuildsDtoWithProvidedValues()
    {
        Team homeTeam = new Team(1, "Home", 1800, false, Confederation.UEFA);
        Team awayTeam = new Team(2, "Away", 1700, false, Confederation.CONMEBOL);
        Stadium stadium = new Stadium("Arena", "City", null, 30000);
        Group group = new Group("A");
        Journey journey = Journey.CreateInstance(1);
        DateTime matchDate = new DateTime(2026, 6, 25);

        MatchGroupDto dto = MatchGroupDto.CreateInstance(
            matchId: 99,
            matchDate: matchDate,
            homeTeamId: homeTeam,
            awayTeamId: awayTeam,
            stadiumId: stadium,
            homeGoals: 3,
            awayGoals: 2,
            group: group,
            journey: journey);

        Assert.AreEqual(99, dto.MatchId);
        Assert.AreEqual(matchDate, dto.MatchDate);
        Assert.AreSame(homeTeam, dto.HomeTeamId);
        Assert.AreSame(awayTeam, dto.AwayTeamId);
        Assert.AreSame(stadium, dto.StadiumId);
        Assert.AreEqual(3, dto.HomeGoals);
        Assert.AreEqual(2, dto.AwayGoals);
        Assert.AreSame(group, dto.Group);
        Assert.AreSame(journey, dto.Journey);
    }

    [TestMethod]
    public void AuditLogFilterDtoCanStoreAllOptionalFilters()
    {
        DateTime from = new DateTime(2026, 6, 1);
        DateTime to = new DateTime(2026, 6, 30);
        AuditLogFilterDto dto = new AuditLogFilterDto
        {
            From = from,
            To = to,
            Action = "CREATE_USER",
            PerformedByEmail = "admin@copa.com"
        };

        Assert.AreEqual(from, dto.From);
        Assert.AreEqual(to, dto.To);
        Assert.AreEqual("CREATE_USER", dto.Action);
        Assert.AreEqual("admin@copa.com", dto.PerformedByEmail);
    }
}
