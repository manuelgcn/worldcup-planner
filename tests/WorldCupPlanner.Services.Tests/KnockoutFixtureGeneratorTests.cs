using WorldCupPlanner.DataAccess.Repositories;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Interfaces;
using Match = WorldCupPlanner.Domain.Match;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class KnockoutFixtureGeneratorTests
{
    [TestMethod]
    public void GeneratePhase2_Returns16RoundOf32Matches_WithoutSameGroupPairings()
    {
        // Arrange
        QualifiedTeams qualifiedTeams = BuildQualifiedTeams();
        MockMatchSimulationEngine simulationEngine = new MockMatchSimulationEngine();
        MockMatchRepository matchRepository = new MockMatchRepository();
        MockTournamentSettingsRepository settingsRepository = new MockTournamentSettingsRepository();
        settingsRepository.Add(new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 77, 99, 0));
        MockStadiumRepository stadiumRepository = new MockStadiumRepository();
        stadiumRepository.Add(new Stadium("S1", "City1", null, 30000) { Id = 1 });
        stadiumRepository.Add(new Stadium("S2", "City2", null, 30000) { Id = 2 });
        stadiumRepository.Add(new Stadium("S3", "City3", null, 30000) { Id = 3 });
        KnockoutFixtureGenerator generator = new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([simulationEngine]),
            matchRepository,
            new MockEliminationMatchRepository(),
            new MockGroupRepository(),
            stadiumRepository,
            settingsRepository,
            new MockAuditLogService(),
            new MockNotificationService(),
            new MockRankingService());

        // Act
        List<Match> result = generator.GeneratePhase2(qualifiedTeams);

        // Assert
        Assert.AreEqual(16, result.Count);
        Assert.IsTrue(result.All(match => match is MatchElimination elimination && elimination.Phase == Phase.RoundOf32));

        foreach (MatchElimination match in result.Cast<MatchElimination>())
        {
            string homeGroup = qualifiedTeams.GroupByTeamId[match.HomeTeam!.Id];
            string awayGroup = qualifiedTeams.GroupByTeamId[match.AwayTeam!.Id];
            Assert.AreNotEqual(homeGroup, awayGroup, $"Found same-group pairing in match {match.MatchId}.");
        }
    }

    [TestMethod]
    public void SimulateNextRound_AppliesKnockoutRankingPerSimulatedMatch()
    {
        // Arrange
        MockMatchSimulationEngine simulationEngine = new MockMatchSimulationEngine();
        MockMatchRepository matchRepository = new MockMatchRepository();
        MockTournamentSettingsRepository settingsRepository = new MockTournamentSettingsRepository();
        settingsRepository.Add(new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 77, 99, 0));
        MockEliminationMatchRepository eliminationRepository = new MockEliminationMatchRepository();
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockStadiumRepository stadiumRepository = new MockStadiumRepository();
        stadiumRepository.Add(new Stadium("S1", "City1", null, 30000) { Id = 1 });
        Mock<IRankingService> rankingServiceMock = new Mock<IRankingService>();

        KnockoutFixtureGenerator generator = new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([simulationEngine]),
            matchRepository,
            eliminationRepository,
            groupRepository,
            stadiumRepository,
            settingsRepository,
            AuditLogService.CreateInstance(new MockAuditLogRepository()),
            new MockNotificationService(),
            rankingServiceMock.Object);

        foreach (Match match in generator.GeneratePhase2(BuildQualifiedTeams()))
        {
            eliminationRepository.Add(match);
        }

        // Act
        generator.SimulateNextRound(null);

        // Assert: la ronda simulada (R32, 16 partidos) debe actualizar el ranking como Knockout
        rankingServiceMock.Verify(
            r => r.ApplyMatchResult(It.IsAny<Match>(), MatchPhase.Knockout),
            Times.Exactly(16));
    }

    private static QualifiedTeams BuildQualifiedTeams()
    {
        List<Group> groups = new List<Group>();
        int teamId = 1;
        for (char name = 'A'; name <= 'L'; name++)
        {
            Group group = new Group(name.ToString());
            for (int i = 0; i < 4; i++)
            {
                group.AddTeam(new Team(teamId, $"T{teamId}", 2000 - teamId, false, Confederation.UEFA));
                teamId++;
            }

            groups.Add(group);
        }

        Dictionary<int, string> groupByTeamId = groups
            .SelectMany(group => group.Teams.Select(team => new { team.Id, group.Name }))
            .ToDictionary(entry => entry.Id, entry => entry.Name);

        List<Team> firsts = groups.Select(group => group.Teams.ElementAt(0)).ToList();
        List<Team> seconds = groups.Select(group => group.Teams.ElementAt(1)).ToList();
        List<Team> thirds = groups.Take(8).Select(group => group.Teams.ElementAt(2)).ToList();

        return new QualifiedTeams
        {
            TopEightFirsts = firsts.Take(8).ToList(),
            RemainingFourFirsts = firsts.Skip(8).Take(4).ToList(),
            TopEightSeconds = seconds.Take(8).ToList(),
            WorstFourSeconds = seconds.Skip(8).Take(4).ToList(),
            BestEightThirds = thirds,
            GroupByTeamId = groupByTeamId
        };
    }
}
