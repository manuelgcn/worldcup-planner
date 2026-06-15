using DataAccess.Repositories;
using ProjectCopa;
using Services;
using Services.Exceptions;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class KnockoutFixtureGeneratorExtensionTests
{
    private const int DefaultSeed = 7;
    private const int DefaultSettingsId = 1;
    private const string AdminEmail = "admin@copa.com";
    private const int TotalGroupCount = 12;
    private const int TeamsPerGroup = 4;
    private const int MatchesPerGroup = 6;
    private const int ExpectedR32Matches = 16;
    private const int ExpectedR16Matches = 8;
    private const int ExpectedQuarterFinalMatches = 4;
    private const int ExpectedSemiFinalMatches = 2;
    private const int ExpectedFinalMatches = 1;
    private const int ExpectedThirdPlaceMatches = 1;

    private static readonly DateTime DefaultStartDate = new DateTime(2026, 6, 1);

    private MockGroupRepository _groupRepository = null!;
    private MockMatchRepository _matchRepository = null!;
    private MockEliminationMatchRepository _eliminationRepository = null!;
    private MockStadiumRepository _stadiumRepository = null!;
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private MockMatchSimulationEngine _simulationEngine = null!;
    private MockUnitOfWork _unitOfWork = null!;
    private MockAuditLogRepository _auditRepository = null!;
    private AuditLogService _auditService = null!;
    private KnockoutFixtureGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _groupRepository = new MockGroupRepository();
        _matchRepository = new MockMatchRepository();
        _eliminationRepository = new MockEliminationMatchRepository();
        _stadiumRepository = new MockStadiumRepository();
        _settingsRepository = new MockTournamentSettingsRepository();
        _simulationEngine = new MockMatchSimulationEngine();
        _unitOfWork = new MockUnitOfWork();
        _auditRepository = new MockAuditLogRepository();
        _auditService = AuditLogService.CreateInstance(_auditRepository);

        SeedStadiums();
        SeedSettings();
        SeedGroups();

        _generator = NewGenerator();
    }

    [TestMethod]
    public void IsBracketGenerated_BeforeGenerateBracket_ReturnsFalse()
    {
        SeedPlayedGroupStageMatches();

        Assert.IsFalse(_generator.IsBracketGenerated());
    }

    [TestMethod]
    public void IsBracketGenerated_AfterGenerateBracket_ReturnsTrue()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_generator.IsBracketGenerated());
    }

    [TestMethod]
    public void GenerateBracket_PersistsSixteenR32MatchesInEliminationRepo()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        IReadOnlyCollection<Match> elimMatches = _eliminationRepository.GetAll();
        Assert.AreEqual(ExpectedR32Matches, elimMatches.Count);
        Assert.IsTrue(elimMatches.All(match => match is MatchElimination elimination && elimination.Phase == Phase.RoundOf32));
    }

    [TestMethod]
    public void GenerateBracket_AuditsGenerateKnockoutAction()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.GenerateKnockout));
    }

    [TestMethod]
    public void GenerateBracket_AuditDetailsContainPerformedByEmail()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateKnockout);
        Assert.AreEqual(AdminEmail, log.PerformedByEmail);
    }

    [TestMethod]
    public void GenerateBracket_AuditDetailsContainSeed()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: 42, performedByEmail: AdminEmail);

        AuditLog log = _auditRepository.GetAll().First(entry => entry.Action == AuditActions.GenerateKnockout);
        Assert.IsTrue(log.Details.Contains("42"), $"Expected seed 42 in details, got '{log.Details}'");
    }

    [TestMethod]
    public void GenerateBracket_WhenAlreadyGenerated_Throws()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateBracket_WhenGroupStageNotComplete_Throws()
    {
        SeedUnplayedGroupStageMatches();

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void GenerateBracket_CallsUnitOfWorkCommit()
    {
        SeedPlayedGroupStageMatches();

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsTrue(_unitOfWork.CommitCalls >= 1);
    }

    [TestMethod]
    public void SimulateNextRound_SimulatesR32MatchesWhenR32IsCurrentRound()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        Assert.AreEqual(ExpectedR32Matches, _simulationEngine.SimulateCalls);
    }

    [TestMethod]
    public void SimulateNextRound_CreatesR16PhaseAfterR32Simulated()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        IReadOnlyCollection<Match> elim = _eliminationRepository.GetAll();
        Assert.AreEqual(ExpectedR16Matches, elim.OfType<MatchElimination>().Count(match => match.Phase == Phase.RoundOf16));
    }

    [TestMethod]
    public void SimulateNextRound_AuditsSimulateRoundAction()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateNextRound(performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.SimulateRound));
    }

    [TestMethod]
    public void SimulateUntilFinal_LeavesAllRoundsCompleteIncludingThirdPlace()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateUntilFinal(performedByEmail: AdminEmail);

        List<MatchElimination> all = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        Assert.AreEqual(ExpectedR32Matches, all.Count(match => match.Phase == Phase.RoundOf32));
        Assert.AreEqual(ExpectedR16Matches, all.Count(match => match.Phase == Phase.RoundOf16));
        Assert.AreEqual(ExpectedQuarterFinalMatches, all.Count(match => match.Phase == Phase.QuarterFinals));
        Assert.AreEqual(ExpectedSemiFinalMatches, all.Count(match => match.Phase == Phase.SemiFinal));
        Assert.AreEqual(ExpectedFinalMatches, all.Count(match => match.Phase == Phase.Final));
        Assert.AreEqual(ExpectedThirdPlaceMatches, all.Count(match => match.Phase == Phase.ThirdPlace));
    }

    [TestMethod]
    public void SimulateUntilFinal_AuditsMultipleSimulateRoundActions()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        _generator.SimulateUntilFinal(performedByEmail: AdminEmail);

        int simulateRoundLogs = _auditRepository.GetAll().Count(log => log.Action == AuditActions.SimulateRound);
        Assert.IsTrue(simulateRoundLogs >= 5, $"Expected at least 5 SimulateRound logs (R32, R16, QF, SF, Final/3PL), got {simulateRoundLogs}");
    }

    [TestMethod]
    public void GenerateBracket_WithCrossingsSeedOverride_DrawMatchesPersistedSeedDraw()
    {
        // H6: el override de semilla debe controlar el sorteo real, no solo el log.
        // Bracket A: semilla persistida 7, override 999. Bracket B: semilla persistida 999, sin override.
        // Si el override se honra de punta a punta, ambos sorteos deben ser identicos.
        List<string> overridden = BuildBracketPairingSignatures(persistedCrossingsSeed: 7, crossingsSeedOverride: 999);
        List<string> persisted = BuildBracketPairingSignatures(persistedCrossingsSeed: 999, crossingsSeedOverride: null);

        CollectionAssert.AreEqual(persisted, overridden);
    }

    [TestMethod]
    public void GenerateBracket_RoundOf32_IsScheduledAfterGroupStage()
    {
        // H3: el R32 no debe solaparse con la fase de grupos.
        SeedPlayedGroupStageMatches();
        DateTime lastGroupStageDate = _matchRepository.GetAll().Max(match => match.MatchDate);

        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        DateTime firstR32Date = _eliminationRepository.GetAll().Min(match => match.MatchDate);
        Assert.IsTrue(
            firstR32Date > lastGroupStageDate,
            $"R32 ({firstR32Date:o}) debe ser posterior al ultimo partido de grupos ({lastGroupStageDate:o}).");
    }

    [TestMethod]
    public void KnockoutRounds_AreScheduledInChronologicalOrder()
    {
        // H3: cada ronda debe jugarse despues de la anterior, con la Final como la mas tardia.
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        _generator.SimulateUntilFinal(performedByEmail: AdminEmail);

        List<MatchElimination> all = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        DateTime maxR32 = all.Where(m => m.Phase == Phase.RoundOf32).Max(m => m.MatchDate);
        DateTime r16 = all.First(m => m.Phase == Phase.RoundOf16).MatchDate;
        DateTime qf = all.First(m => m.Phase == Phase.QuarterFinals).MatchDate;
        DateTime sf = all.First(m => m.Phase == Phase.SemiFinal).MatchDate;
        DateTime thirdPlace = all.First(m => m.Phase == Phase.ThirdPlace).MatchDate;
        DateTime final = all.First(m => m.Phase == Phase.Final).MatchDate;

        Assert.IsTrue(maxR32 < r16, "R16 debe ser posterior al R32.");
        Assert.IsTrue(r16 < qf, "Cuartos debe ser posterior al R16.");
        Assert.IsTrue(qf < sf, "Semis debe ser posterior a Cuartos.");
        Assert.IsTrue(sf < thirdPlace, "Tercer puesto debe ser posterior a Semis.");
        Assert.IsTrue(sf < final, "Final debe ser posterior a Semis.");
        Assert.AreEqual(all.Max(m => m.MatchDate), final, "La Final debe ser el partido mas tardio.");
    }

    [TestMethod]
    public void CanAdvanceToNextRound_WhenBracketNotGenerated_ReturnsFalse()
    {
        SeedPlayedGroupStageMatches();

        Assert.IsFalse(_generator.CanAdvanceToNextRound());
    }

    [TestMethod]
    public void CanAdvanceToNextRound_WhenR32StillPending_ReturnsFalse()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.IsFalse(_generator.CanAdvanceToNextRound());
    }

    [TestMethod]
    public void CanAdvanceToNextRound_WhenR32FullyPlayed_ReturnsTrue()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);

        Assert.IsTrue(_generator.CanAdvanceToNextRound());
    }

    [TestMethod]
    public void AdvanceToNextRound_WhenBracketNotGenerated_Throws()
    {
        SeedPlayedGroupStageMatches();

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.AdvanceToNextRound(performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void AdvanceToNextRound_WhenCurrentRoundIncomplete_Throws()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.AdvanceToNextRound(performedByEmail: AdminEmail));
    }

    [TestMethod]
    public void AdvanceToNextRound_AfterR32PlayedManually_CreatesEightR16Matches()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);

        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);

        List<MatchElimination> r16 = _eliminationRepository.GetAll()
            .OfType<MatchElimination>()
            .Where(match => match.Phase == Phase.RoundOf16)
            .ToList();
        Assert.AreEqual(ExpectedR16Matches, r16.Count);
    }

    [TestMethod]
    public void AdvanceToNextRound_LeavesNextRoundPendingWithoutSimulating()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);
        int simulateCallsBeforeAdvance = _simulationEngine.SimulateCalls;

        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);

        List<MatchElimination> r16 = _eliminationRepository.GetAll()
            .OfType<MatchElimination>()
            .Where(match => match.Phase == Phase.RoundOf16)
            .ToList();
        Assert.IsTrue(r16.All(match => !match.HasResult), "El R16 generado debe quedar pendiente.");
        Assert.AreEqual(simulateCallsBeforeAdvance, _simulationEngine.SimulateCalls,
            "Avanzar de fase no debe invocar el motor de simulacion.");
    }

    [TestMethod]
    public void AdvanceToNextRound_AuditsAdvanceRoundAction()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);

        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);

        Assert.IsTrue(_auditRepository.GetAll().Any(log => log.Action == AuditActions.AdvanceRound));
    }

    [TestMethod]
    public void AdvanceToNextRound_FromSemiFinals_CreatesPendingFinalAndThirdPlace()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf16);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.QuarterFinals);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.SemiFinal);

        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);

        List<MatchElimination> all = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        Assert.AreEqual(ExpectedFinalMatches, all.Count(match => match.Phase == Phase.Final));
        Assert.AreEqual(ExpectedThirdPlaceMatches, all.Count(match => match.Phase == Phase.ThirdPlace));
        Assert.IsTrue(
            all.Where(match => match.Phase == Phase.Final || match.Phase == Phase.ThirdPlace).All(match => !match.HasResult),
            "La final y el tercer puesto deben quedar pendientes.");
    }

    [TestMethod]
    public void AdvanceToNextRound_WhenFinalAlreadyExists_Throws()
    {
        SeedPlayedGroupStageMatches();
        _generator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf32);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.RoundOf16);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.QuarterFinals);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);
        PlayPhaseManually(Phase.SemiFinal);
        _generator.AdvanceToNextRound(performedByEmail: AdminEmail);

        Assert.ThrowsException<MatchServicesException>(
            () => _generator.AdvanceToNextRound(performedByEmail: AdminEmail));
    }

    // Carga resultados a mano (no simulados) en todos los partidos de una fase,
    // con un marcador no empatado para que GetWinner() devuelva un ganador claro.
    private void PlayPhaseManually(Phase phase)
    {
        List<MatchElimination> phaseMatches = _eliminationRepository.GetAll()
            .OfType<MatchElimination>()
            .Where(match => match.Phase == phase)
            .ToList();

        foreach (MatchElimination match in phaseMatches)
        {
            match.SetGoals(2, 1);
            _eliminationRepository.Update(match);
        }
    }

    private static List<string> BuildBracketPairingSignatures(int persistedCrossingsSeed, int? crossingsSeedOverride)
    {
        MockGroupRepository groupRepository = new MockGroupRepository();
        MockMatchRepository matchRepository = new MockMatchRepository();
        MockEliminationMatchRepository eliminationRepository = new MockEliminationMatchRepository();
        MockStadiumRepository stadiumRepository = new MockStadiumRepository();
        MockTournamentSettingsRepository settingsRepository = new MockTournamentSettingsRepository();

        for (int index = 0; index < 4; index++)
        {
            stadiumRepository.Add(new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 });
        }

        settingsRepository.Add(new TournamentSettings(
            DefaultSettingsId, DefaultStartDate, 3, 3, DefaultSeed, persistedCrossingsSeed, DefaultSeed, DefaultSeed));

        SeedGroupsInto(groupRepository);
        SeedPlayedGroupStageMatchesInto(groupRepository, matchRepository, stadiumRepository);

        KnockoutFixtureGenerator generator = new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([new MockMatchSimulationEngine()]),
            matchRepository,
            eliminationRepository,
            groupRepository,
            stadiumRepository,
            settingsRepository,
            new MockUnitOfWork(),
            AuditLogService.CreateInstance(new MockAuditLogRepository()),
            new MockNotificationService(),
            new MockRankingService());

        generator.GenerateBracket(crossingsSeedOverride, performedByEmail: AdminEmail);

        return eliminationRepository.GetAll()
            .OfType<MatchElimination>()
            .Select(match =>
            {
                int low = Math.Min(match.HomeTeam!.Id, match.AwayTeam!.Id);
                int high = Math.Max(match.HomeTeam!.Id, match.AwayTeam!.Id);
                return $"{low}-{high}";
            })
            .OrderBy(signature => signature)
            .ToList();
    }

    private static void SeedGroupsInto(MockGroupRepository groupRepository)
    {
        int teamId = 1;
        for (int groupIndex = 0; groupIndex < TotalGroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);
            for (int teamIndex = 0; teamIndex < TeamsPerGroup; teamIndex++)
            {
                group.AddTeam(new Team(teamId, $"T{teamId:D2}", 2500 - teamId, false, Confederation.UEFA));
                teamId++;
            }
            groupRepository.Add(group);
        }
    }

    private static void SeedPlayedGroupStageMatchesInto(
        MockGroupRepository groupRepository,
        MockMatchRepository matchRepository,
        MockStadiumRepository stadiumRepository)
    {
        Stadium stadium = stadiumRepository.GetAll().First();
        int matchId = 1;
        foreach (Group group in groupRepository.GetAll())
        {
            List<Team> teams = group.Teams.ToList();
            (int home, int away)[] pairs = { (0, 3), (1, 2), (0, 2), (1, 3), (0, 1), (2, 3) };
            for (int matchIndex = 0; matchIndex < MatchesPerGroup; matchIndex++)
            {
                int journeyNumber = (matchIndex / 2) + 1;
                Journey journey = Journey.CreateInstance(journeyNumber);
                (int home, int away) pair = pairs[matchIndex];
                MatchGroup match = MatchGroup.CreateInstance(
                    id: matchId++,
                    matchDate: DefaultStartDate.AddDays(journeyNumber * 3),
                    homeTeam: teams[pair.home],
                    awayTeam: teams[pair.away],
                    stadium: stadium,
                    group: group,
                    journey: journey);
                match.SetMatchCode($"M{match.MatchId}");
                match.SetGoals(2, 1);
                matchRepository.Add(match);
            }
        }
    }

    private KnockoutFixtureGenerator NewGenerator()
    {
        return new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([_simulationEngine]),
            _matchRepository,
            _eliminationRepository,
            _groupRepository,
            _stadiumRepository,
            _settingsRepository,
            _unitOfWork,
            _auditService,
            new MockNotificationService(),
            new MockRankingService());
    }

    private void SeedStadiums()
    {
        for (int index = 0; index < 4; index++)
        {
            Stadium stadium = new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 };
            _stadiumRepository.Add(stadium);
        }
    }

    private void SeedSettings()
    {
        _settingsRepository.Add(new TournamentSettings(
            id: DefaultSettingsId,
            startDate: DefaultStartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: DefaultSeed,
            crossingsSeed: DefaultSeed,
            simulationSeed: DefaultSeed,
            completionSeed: DefaultSeed));
    }

    private void SeedGroups()
    {
        int teamId = 1;
        for (int groupIndex = 0; groupIndex < TotalGroupCount; groupIndex++)
        {
            string groupName = ((char)('A' + groupIndex)).ToString();
            Group group = new Group(groupName);
            for (int teamIndex = 0; teamIndex < TeamsPerGroup; teamIndex++)
            {
                Team team = new Team(teamId, $"T{teamId:D2}", 2500 - teamId, false, Confederation.UEFA);
                group.AddTeam(team);
                teamId++;
            }
            _groupRepository.Add(group);
        }
    }

    private void SeedPlayedGroupStageMatches()
    {
        AddGroupStageMatches(homeGoals: 2, awayGoals: 1);
    }

    // Sin resultado registrado: los partidos quedan sin goles ni simulacion, por lo que
    // Match.HasResult es false y la fase de grupos sigue incompleta.
    private void SeedUnplayedGroupStageMatches()
    {
        AddGroupStageMatches(homeGoals: null, awayGoals: null);
    }

    private void AddGroupStageMatches(int? homeGoals, int? awayGoals)
    {
        Stadium stadium = _stadiumRepository.GetAll().First();
        int matchId = 1;
        foreach (Group group in _groupRepository.GetAll())
        {
            List<Team> teams = group.Teams.ToList();
            (int home, int away)[] pairs =
            {
                (0, 3), (1, 2),
                (0, 2), (1, 3),
                (0, 1), (2, 3)
            };

            int journeyIndex = 0;
            for (int matchIndex = 0; matchIndex < MatchesPerGroup; matchIndex++)
            {
                int journeyNumber = (matchIndex / 2) + 1;
                Journey journey = Journey.CreateInstance(journeyNumber);
                (int home, int away) pair = pairs[matchIndex];
                MatchGroup match = MatchGroup.CreateInstance(
                    id: matchId++,
                    matchDate: DefaultStartDate.AddDays(journeyNumber * 3),
                    homeTeam: teams[pair.home],
                    awayTeam: teams[pair.away],
                    stadium: stadium,
                    group: group,
                    journey: journey);
                match.SetMatchCode($"M{match.MatchId}");
                if (homeGoals.HasValue && awayGoals.HasValue)
                {
                    match.SetGoals(homeGoals.Value, awayGoals.Value);
                }
                _matchRepository.Add(match);
                journeyIndex++;
            }
        }
    }
}
