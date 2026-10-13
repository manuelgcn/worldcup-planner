using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;

namespace WorldCupPlanner.DataAccess.Tests;

// Tests de INTEGRACION de persistencia sobre SQLite en memoria (base relacional real que SI valida
// foreign keys). Reproducen y blindan el bug "simular la fase de grupos rompe la app":
//
//   Al generar la fase 2, cada partido de R32 se fabricaba con partidos fuente SINTETICOS (ids
//   negativos, grupos "X"/"Y") solo para satisfacer la invariante del dominio. Con los repos en
//   memoria era inofensivo; con EF Core, MatchEf.Add marcaba esas fuentes como Unchanged y el
//   INSERT del R32 salia con MatchHomeId = -2000 -> violacion de FK Matches->Matches -> la app se
//   rompia. La suite seguia verde porque el provider InMemory NO valida FKs: de ahi que el defecto
//   llegara a la app. Estos tests cablean los repositorios EF reales con los servicios
//   orquestadores (FixtureOrchestrator, TournamentSimulationService, KnockoutFixtureGenerator).
[TestClass]
public sealed class KnockoutPersistenceIntegrationTests
{
    private const int Seed = 42;
    private const int SettingsId = 1;
    private const int TeamCount = 48;
    private const int StadiumCount = 4;
    private const int ExpectedRoundOf32Matches = 16;
    private const string AdminEmail = "admin@copa.com";
    private static readonly DateTime StartDate = new DateTime(2026, 6, 1);

    private SqliteDbContextFactory _factory = null!;
    private TeamRepository _teamRepository = null!;
    private StadiumRepository _stadiumRepository = null!;
    private TournamentSettingsRepository _settingsRepository = null!;
    private GroupRepository _groupRepository = null!;
    private JourneyRepository _journeyRepository = null!;
    private MatchRepository _matchRepository = null!;
    private EliminationMatchRepository _eliminationRepository = null!;
    private AuditLogRepository _auditRepository = null!;
    private AuditLogService _auditService = null!;
    private ProbabilisticSimulationEngine _engine = null!;
    private KnockoutFixtureGenerator _knockoutGenerator = null!;
    private FixtureOrchestrator _fixtureOrchestrator = null!;
    private TournamentSimulationService _simulationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new SqliteDbContextFactory();
        _teamRepository = new TeamRepository(_factory);
        _stadiumRepository = new StadiumRepository(_factory);
        _settingsRepository = new TournamentSettingsRepository(_factory);
        _groupRepository = new GroupRepository(_factory);
        _journeyRepository = new JourneyRepository(_factory);
        _matchRepository = new MatchRepository(_factory);
        _eliminationRepository = new EliminationMatchRepository(_factory);
        _auditRepository = new AuditLogRepository(_factory);
        _auditService = AuditLogService.CreateInstance(_auditRepository);
        _engine = new ProbabilisticSimulationEngine();

        _knockoutGenerator = new KnockoutFixtureGenerator(
            new SimulationEngineRegistry([_engine]),
            _matchRepository,
            _eliminationRepository,
            _groupRepository,
            _stadiumRepository,
            _settingsRepository,
            _auditService,
            new NotificationService(
                new NotificationRepository(_factory),
                new UserRepository(_factory),
                _auditService),
            new RankingService(_auditService, _teamRepository));

        _fixtureOrchestrator = new FixtureOrchestrator(
            _teamRepository,
            _stadiumRepository,
            _settingsRepository,
            _groupRepository,
            _journeyRepository,
            _matchRepository,
            _eliminationRepository,
            _auditService,
            new SimulationEngineRegistry([_engine]),
            new RankingService(_auditService, _teamRepository));

        _simulationService = new TournamentSimulationService(
            _groupRepository,
            _matchRepository,
            _eliminationRepository,
            new SimulationEngineRegistry([_engine]),
            _auditService,
            _settingsRepository,
            _knockoutGenerator,
            new NotificationService(
                new NotificationRepository(_factory),
                new UserRepository(_factory),
                _auditService),
            new RankingService(_auditService, _teamRepository));

        SeedSettings();
        SeedStadiums(StadiumCount);
        SeedTeams(TeamCount);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void SimulateGroupStageAndGenerateKnockout_PersistsRoundOf32WithoutSyntheticSources()
    {
        _fixtureOrchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        // Camino del boton "Simular" (Simulator.razor): simula los 72 partidos de grupos y genera y
        // persiste los 16 partidos de R32. Antes del fix, esta linea rompia con una violacion de FK.
        _simulationService.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);

        List<MatchElimination> roundOf32 = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        Assert.AreEqual(ExpectedRoundOf32Matches, roundOf32.Count);
        Assert.IsTrue(roundOf32.All(match => match.Phase == Phase.RoundOf32));

        // R32 es la ronda inicial del bracket: NO tiene partidos fuente (MatchHome/MatchAway null).
        Assert.IsTrue(
            roundOf32.All(match => match.MatchHome == null && match.MatchAway == null),
            "RoundOf32 matches must persist with NULL source FKs (no synthetic source matches).");

        // No quedo ningun partido sintetico ni id negativo en la tabla TPH.
        Assert.IsFalse(_matchRepository.GetAll().Any(match => match.MatchId < 0));
    }

    [TestMethod]
    public void GenerateBracket_AfterGroupStageSimulated_PersistsRoundOf32WithoutSyntheticSources()
    {
        // Camino alternativo (pagina Fixture): el mismo EliminationBuilder.CreateRoundOf32, antes
        // tambien rompia aca. Se simula la fase de grupos sin generar la fase 2 y luego se genera el
        // bracket por separado.
        _fixtureOrchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        SimulateGroupStageOnly();

        _knockoutGenerator.GenerateBracket(crossingsSeedOverride: null, performedByEmail: AdminEmail);

        List<MatchElimination> roundOf32 = _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
        Assert.AreEqual(ExpectedRoundOf32Matches, roundOf32.Count);
        Assert.IsTrue(roundOf32.All(match => match.MatchHome == null && match.MatchAway == null));
    }

    [TestMethod]
    public void SimulateUntilFinal_ProducesPersistedChampionAndBracketChainThatSurvivesReload()
    {
        _fixtureOrchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        _simulationService.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);

        // Simula ronda a ronda hasta la Final. R16 en adelante SI tienen fuentes reales (los partidos
        // de la ronda previa, ya persistidos): el guard de MatchEf debe dejarlas pasar.
        _knockoutGenerator.SimulateUntilFinal(AdminEmail);

        // Relee desde un repositorio NUEVO sobre la misma conexion SQLite (simula reiniciar la app).
        List<MatchElimination> all = new EliminationMatchRepository(_factory)
            .GetAll().OfType<MatchElimination>().ToList();

        MatchElimination final = all.Single(match => match.Phase == Phase.Final);
        Assert.IsNotNull(final.GetWinner(), "The final must resolve a champion after reload.");
        Assert.IsTrue(all.Any(match => match.Phase == Phase.ThirdPlace));

        // R32 (ronda inicial) sin fuentes; las rondas posteriores con su cadena de fuentes persistida.
        Assert.IsTrue(all.Where(match => match.Phase == Phase.RoundOf32).All(match => match.MatchHome == null));
        Assert.IsTrue(all.Where(match => match.Phase == Phase.RoundOf16).All(match => match.MatchHome != null));
    }

    [TestMethod]
    public void DeleteGroupStage_AfterKnockoutGenerated_RemovesEverythingAndSurvivesReload()
    {
        _fixtureOrchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        _simulationService.SimulateGroupStageAndGenerateKnockout(Seed, AdminEmail);
        Assert.AreEqual(ExpectedRoundOf32Matches, _eliminationRepository.GetAll().Count, "Precondicion: el knockout debe existir.");

        // Borrado total: en SQLite (provider relacional que SI valida FKs) debe borrar la fase 2 y la
        // fase de grupos sin violar la FK autorreferente Matches->Matches.
        _fixtureOrchestrator.DeleteGroupStage(performedByEmail: AdminEmail);

        Assert.AreEqual(0, _groupRepository.GetAll().Count);
        Assert.AreEqual(0, _journeyRepository.GetAll().Count);
        Assert.AreEqual(0, _matchRepository.GetAll().Count);
        Assert.AreEqual(0, _eliminationRepository.GetAll().Count);

        // Relee desde repositorios NUEVOS sobre la misma conexion SQLite (simula reiniciar la app).
        Assert.AreEqual(0, new MatchRepository(_factory).GetAll().Count);
        Assert.AreEqual(0, new EliminationMatchRepository(_factory).GetAll().Count);
        Assert.AreEqual(0, new GroupRepository(_factory).GetAll().Count);
    }

    [TestMethod]
    public void RegenerateGroupStage_AfterPlayedMatches_DrawsTheSameGroupsAsTheFirstGeneration()
    {
        // Con EF cada GetAll devuelve instancias nuevas: los bombos tienen que salir del ranking ya
        // revertido, no de los equipos leidos antes de revertir el Elo de los partidos jugados.
        _fixtureOrchestrator.GenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);
        Dictionary<string, string> firstDraw = GroupComposition();
        _simulationService.SimulateGroupStage(Seed, AdminEmail);

        _fixtureOrchestrator.RegenerateGroupStage(fixtureSeedOverride: null, selectedEngineName: null, performedByEmail: AdminEmail);

        CollectionAssert.AreEquivalent(firstDraw, GroupComposition());
    }

    private Dictionary<string, string> GroupComposition()
    {
        return new GroupRepository(_factory).GetAll().ToDictionary(
            group => group.Name,
            group => string.Join(",", group.Teams.Select(team => team.Id).OrderBy(id => id)));
    }

    // Simula los 72 partidos de grupos (sin generar la fase 2), dejandolos marcados como jugados.
    private void SimulateGroupStageOnly()
    {
        foreach (Match match in _matchRepository.GetAll().OfType<MatchGroup>().ToList())
        {
            _engine.Simulate(match, Seed);
            _matchRepository.Update(match);
        }
    }

    private void SeedSettings()
    {
        _settingsRepository.Add(new TournamentSettings(
            SettingsId,
            StartDate,
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: Seed,
            crossingsSeed: Seed,
            simulationSeed: Seed,
            completionSeed: Seed));
    }

    private void SeedStadiums(int count)
    {
        for (int index = 0; index < count; index++)
        {
            _stadiumRepository.Add(new Stadium($"Stadium {(char)('A' + index)}", "City", null, 30000) { Id = index + 1 });
        }
    }

    // 48 equipos repartidos en las 6 confederaciones y en 4 tramos de ranking, igual que
    // FixtureOrchestratorTests: distribucion que el generador de fixture acepta sin conflictos de cupo.
    private void SeedTeams(int count)
    {
        Confederation[] confederations = Enum.GetValues<Confederation>();
        const int teamsPerRankingTier = 12;
        for (int teamIndex = 0; teamIndex < count; teamIndex++)
        {
            int tier = teamIndex / teamsPerRankingTier;
            _teamRepository.Add(new Team(
                id: teamIndex + 1,
                name: $"Team-{teamIndex + 1:D2}",
                fifaRanking: 2000 - (tier * 100),
                isFictitious: false,
                confederation: confederations[teamIndex % confederations.Length]));
        }
    }
}
