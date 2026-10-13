using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.Services;
using Match = WorldCupPlanner.Domain.Match;

namespace WorldCupPlanner.Services.Tests;

// Bug 1: al generar octavos (8) y cuartos (4) todos los partidos de la ronda recibian el mismo
// estadio y la misma fecha/hora, colisionando. Estos tests verifican que cada ronda eliminatoria
// creada a partir de los ganadores no repita la terna (estadio, fecha/hora) y que ningun dia
// supere MaxMatchesPerDay.
[TestClass]
public sealed class KnockoutRoundScheduleCollisionTests
{
    private const int MaxMatchesPerDay = 3;

    private MockEliminationMatchRepository _eliminationRepository = null!;
    private KnockoutFixtureGenerator _generator = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        MockTournamentSettingsRepository settingsRepository = new MockTournamentSettingsRepository();
        settingsRepository.Add(new TournamentSettings(1, new DateTime(2026, 6, 1), MaxMatchesPerDay, 3, 0, 77, 99, 0));

        MockStadiumRepository stadiumRepository = new MockStadiumRepository();
        stadiumRepository.Add(new Stadium("S1", "City1", null, 30000) { Id = 1 });
        stadiumRepository.Add(new Stadium("S2", "City2", null, 30000) { Id = 2 });
        stadiumRepository.Add(new Stadium("S3", "City3", null, 30000) { Id = 3 });
        stadiumRepository.Add(new Stadium("S4", "City4", null, 30000) { Id = 4 });

        _eliminationRepository = new MockEliminationMatchRepository();

        _generator = new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([new MockMatchSimulationEngine()]),
            new MockMatchRepository(),
            _eliminationRepository,
            new MockGroupRepository(),
            stadiumRepository,
            settingsRepository,
            new MockAuditLogService(),
            new MockNotificationService(),
            new MockRankingService());

        foreach (Match match in _generator.GeneratePhase2(BuildQualifiedTeams()))
        {
            _eliminationRepository.Add(match);
        }
    }

    [TestMethod]
    public void AdvancingThroughRounds_NeverRepeatsStadiumDateAndRespectsMaxMatchesPerDay()
    {
        // R32 ya esta en el repo: se simula y se crea la siguiente ronda (octavos) pendiente.
        _generator.SimulateNextRound(null);
        _generator.AdvanceToNextRound(null);
        AssertRoundHasNoScheduleCollision(Phase.RoundOf16, expectedMatches: 8);

        _generator.SimulateNextRound(null);
        _generator.AdvanceToNextRound(null);
        AssertRoundHasNoScheduleCollision(Phase.QuarterFinals, expectedMatches: 4);

        _generator.SimulateNextRound(null);
        _generator.AdvanceToNextRound(null);
        AssertRoundHasNoScheduleCollision(Phase.SemiFinal, expectedMatches: 2);
    }

    private void AssertRoundHasNoScheduleCollision(Phase phase, int expectedMatches)
    {
        List<MatchElimination> roundMatches = _eliminationRepository.GetAll()
            .OfType<MatchElimination>()
            .Where(match => match.Phase == phase)
            .ToList();

        Assert.AreEqual(expectedMatches, roundMatches.Count, $"Cantidad inesperada de partidos en {phase}.");

        int distinctStadiumDate = roundMatches
            .Select(match => (StadiumId: match.Stadium!.Id, match.MatchDate))
            .Distinct()
            .Count();
        Assert.AreEqual(
            roundMatches.Count,
            distinctStadiumDate,
            $"Dos o mas partidos de {phase} comparten (estadio, fecha/hora).");

        foreach (IGrouping<DateTime, MatchElimination> perDay in roundMatches.GroupBy(match => match.MatchDate.Date))
        {
            Assert.IsTrue(
                perDay.Count() <= MaxMatchesPerDay,
                $"El dia {perDay.Key:d} de {phase} tiene {perDay.Count()} partidos (> {MaxMatchesPerDay}).");
        }
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
