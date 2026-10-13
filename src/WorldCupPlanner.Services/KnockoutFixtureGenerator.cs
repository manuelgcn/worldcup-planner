using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class KnockoutFixtureGenerator : IKnockoutFixtureGenerator
{
    private const int RequiredGroupCount = 12;
    private const int RequiredGroupStageMatchCount = 72;
    private const int FirstNewMatchIdSeed = 3000;
    private const int DaysFromStartToFirstKnockoutRound = 60;
    private const string TournamentSettingsRequiredMessage = "La configuracion del torneo es obligatoria para generar la fase 2.";
    private const string StadiumsRequiredMessage = "Se requiere al menos un estadio para generar la fase 2.";
    private const string GroupsRequiredMessage = "Deben existir los 12 grupos antes de generar el cuadro eliminatorio.";
    private const string GroupStageIncompleteMessage = "La fase de grupos no esta completa: todos los partidos deben tener un resultado cargado.";
    private const string BracketAlreadyGeneratedMessage = "El cuadro eliminatorio ya fue generado.";
    private const string BracketNotGeneratedMessage = "El cuadro eliminatorio aun no fue generado.";
    private const string BracketAlreadyCompleteMessage = "El cuadro eliminatorio ya llego a la final: no hay mas fases para avanzar.";
    private const string MissingGroupForTeamMessagePrefix = "No se encontro grupo para el equipo ";
    private const string MatchWithoutWinnerMessage = "No se puede avanzar de fase: el partido {0} no tiene ganador.";

    private readonly ISimulationEngineRegistry _simulationEngineRegistry;
    private readonly IMatchRepository _matchRepository;
    private readonly IEliminationMatchRepository _eliminationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;
    private readonly IAuditLogService _auditLog;
    private readonly IStadiumRepository _stadiumRepository;
    private readonly INotificationService _notificationService;
    private readonly IRankingService _rankingService;

    public KnockoutFixtureGenerator(
        ISimulationEngineRegistry simulationEngineRegistry,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        IGroupRepository groupRepository,
        IStadiumRepository stadiumRepository,
        ITournamentSettingsRepository settingsRepository,
        IAuditLogService auditLog,
        INotificationService notificationService,
        IRankingService rankingService)
    {
        ArgumentNullException.ThrowIfNull(simulationEngineRegistry);
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(eliminationRepository);
        ArgumentNullException.ThrowIfNull(groupRepository);
        ArgumentNullException.ThrowIfNull(stadiumRepository);
        ArgumentNullException.ThrowIfNull(settingsRepository);
        ArgumentNullException.ThrowIfNull(auditLog);
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(rankingService);
        _simulationEngineRegistry = simulationEngineRegistry;
        _matchRepository = matchRepository;
        _eliminationRepository = eliminationRepository;
        _groupRepository = groupRepository;
        _stadiumRepository = stadiumRepository;
        _settingsRepository = settingsRepository;
        _auditLog = auditLog;
        _notificationService = notificationService;
        _rankingService = rankingService;
    }

    public List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams)
    {
        ArgumentNullException.ThrowIfNull(qualifiedTeams);

        TournamentSettings settings = LoadSettingsOrThrow();
        return BuildRoundOf32(qualifiedTeams, settings);
    }

    // Construye el R32 usando EXACTAMENTE la configuracion recibida (incluida su CrossingsSeed),
    // sin recargar settings: asi la clasificacion, el sorteo y la auditoria comparten la misma
    // semilla efectiva cuando se pasa un override desde GenerateBracket (H6).
    private List<Match> BuildRoundOf32(QualifiedTeams qualifiedTeams, TournamentSettings settings)
    {
        List<Stadium> stadiums = _stadiumRepository.GetAll().ToList();
        if (stadiums.Count == 0)
        {
            throw new KnockoutGeneratorException(StadiumsRequiredMessage);
        }

        EnsureEveryQualifiedTeamHasGroup(qualifiedTeams);

        EliminationBuilder builder = new EliminationBuilder();
        int maxPerDay = settings.MaxMatchesPerDay;
        // El R32 arranca al terminar la fase de grupos (H3): nunca se solapa con ella.
        DateTime roundStart = settings.StartDate.AddDays(KnockoutPhaseOffsetDays(Phase.RoundOf32));
        Func<int, Stadium> stadiumProvider = matchIndex => stadiums[matchIndex % stadiums.Count];
        Func<int, DateTime> dateProvider = matchIndex => roundStart.AddDays(matchIndex / maxPerDay);

        // H2: se pasa QualifiedTeams (no la lista aplanada) para respetar las categorias A1..B8.
        List<MatchElimination> roundOf32 = builder.CreateRoundOf32(
            qualifiedTeams,
            stadiums,
            settings,
            settings.CrossingsSeed,
            stadiumProvider,
            dateProvider);

        return roundOf32.Cast<Match>().ToList();
    }

    public bool IsBracketGenerated()
    {
        return _eliminationRepository.GetAll().Count > 0;
    }

    public void GenerateBracket(int? crossingsSeedOverride, string? performedByEmail)
    {

        EnsureBracketNotYetGenerated();

        TournamentSettings settings = LoadSettingsOrThrow();
        IReadOnlyCollection<Group> groups = _groupRepository.GetAll();
        EnsureTwelveGroups(groups);
        EnsureGroupStageComplete();

        int effectiveSeed = crossingsSeedOverride ?? settings.CrossingsSeed;
        TournamentSettings settingsForBracket = WithCrossingsSeed(settings, effectiveSeed);

        QualifiedTeams qualifiedTeams = ComputeQualifiedTeams(groups, settingsForBracket);
        List<Match> roundOf32 = BuildRoundOf32(qualifiedTeams, settingsForBracket);

        foreach (Match match in roundOf32)
        {
            _eliminationRepository.Add(match);
        }


        _auditLog.LogPerformedBy(
            AuditActions.GenerateKnockout,
            $"Knockout bracket generated with seed {effectiveSeed}.",
            performedByEmail,
            nameof(KnockoutFixtureGenerator));
    }

    public void SimulateNextRound(string? performedByEmail)
    {
        EnsureBracketGenerated();
        TournamentSettings settings = LoadSettingsOrThrow();

        List<MatchElimination> all = LoadAllEliminationMatches();

        if (TrySimulatePhase(all, Phase.RoundOf32, settings, performedByEmail))
        {
            return;
        }

        Phase[] phasesAfterR32 = { Phase.RoundOf16, Phase.QuarterFinals, Phase.SemiFinal };
        foreach (Phase phase in phasesAfterR32)
        {
            if (TryCreateAndSimulatePhaseFromPrevious(all, phase, settings, performedByEmail))
            {
                return;
            }

            all = LoadAllEliminationMatches();

            if (TrySimulatePhase(all, phase, settings, performedByEmail))
            {
                return;
            }
        }

        TryCreateAndSimulateFinalAndThirdPlace(all, settings, performedByEmail);
    }

    // Fases que pueden originar un avance manual hacia su fase siguiente, de la mas
    // avanzada a la mas temprana. La SemiFinal se trata aparte porque deriva en dos
    // partidos (Final + Tercer puesto), no en una unica ronda.
    private static readonly Phase[] AdvanceableSourcePhases =
        { Phase.QuarterFinals, Phase.RoundOf16, Phase.RoundOf32 };

    public bool CanAdvanceToNextRound()
    {
        if (!IsBracketGenerated())
        {
            return false;
        }

        List<MatchElimination> all = LoadAllEliminationMatches();
        if (all.Count == 0 || all.Any(match => match.Phase == Phase.Final))
        {
            return false;
        }

        List<MatchElimination> semiFinals = all.Where(match => match.Phase == Phase.SemiFinal).ToList();
        if (semiFinals.Count > 0)
        {
            return semiFinals.All(IsMatchPlayed);
        }

        foreach (Phase phase in AdvanceableSourcePhases)
        {
            List<MatchElimination> phaseMatches = all.Where(match => match.Phase == phase).ToList();
            if (phaseMatches.Count == 0)
            {
                continue;
            }

            return phaseMatches.All(IsMatchPlayed);
        }

        return false;
    }

    public void AdvanceToNextRound(string? performedByEmail)
    {
        EnsureBracketGenerated();
        TournamentSettings settings = LoadSettingsOrThrow();
        List<MatchElimination> all = LoadAllEliminationMatches();

        if (all.Any(match => match.Phase == Phase.Final))
        {
            throw new MatchServicesException(BracketAlreadyCompleteMessage);
        }

        // La SemiFinal completa habilita crear la Final y el Tercer puesto, ambos pendientes.
        List<MatchElimination> semiFinals = all.Where(match => match.Phase == Phase.SemiFinal).ToList();
        if (semiFinals.Count > 0)
        {
            EnsurePhaseFullyPlayed(semiFinals, Phase.SemiFinal);

            // Se agrega la Final ANTES de crear el Tercer puesto: ambos derivan su MatchId de
            // NextMatchId(), por lo que persistir la Final primero evita que compartan id.
            List<MatchElimination> finalMatches = CreateRoundFromWinners(semiFinals, Phase.Final, settings);
            foreach (MatchElimination match in finalMatches)
            {
                _eliminationRepository.Add(match);
            }

            List<MatchElimination> thirdPlaceMatches = CreateRoundFromLosers(semiFinals, Phase.ThirdPlace, settings);
            foreach (MatchElimination match in thirdPlaceMatches)
            {
                _eliminationRepository.Add(match);
            }

            CommitAndAuditAdvance(Phase.Final, performedByEmail);
            return;
        }

        // Caso general R32→R16, R16→Cuartos, Cuartos→Semis: se toma la fase mas avanzada
        // existente, se exige que este completa y se crea la siguiente dejandola pendiente.
        foreach (Phase phase in AdvanceableSourcePhases)
        {
            List<MatchElimination> phaseMatches = all.Where(match => match.Phase == phase).ToList();
            if (phaseMatches.Count == 0)
            {
                continue;
            }

            EnsurePhaseFullyPlayed(phaseMatches, phase);
            Phase next = NextPhase(phase);
            List<MatchElimination> created = CreateRoundFromWinners(phaseMatches, next, settings);
            PersistAndAuditAdvance(created, next, performedByEmail);
            return;
        }

        throw new MatchServicesException(BracketNotGeneratedMessage);
    }

    private void PersistAndAuditAdvance(
        List<MatchElimination> created,
        Phase createdPhase,
        string? performedByEmail)
    {
        foreach (MatchElimination match in created)
        {
            _eliminationRepository.Add(match);
        }

        CommitAndAuditAdvance(createdPhase, performedByEmail);
    }

    private void CommitAndAuditAdvance(Phase createdPhase, string? performedByEmail)
    {
        _auditLog.LogPerformedBy(
            AuditActions.AdvanceRound,
            $"Advanced bracket to phase {createdPhase} (pending manual results).",
            performedByEmail,
            nameof(KnockoutFixtureGenerator));
    }

    private static void EnsurePhaseFullyPlayed(List<MatchElimination> matches, Phase phase)
    {
        if (matches.Any(match => !IsMatchPlayed(match)))
        {
            throw new MatchServicesException(
                $"No se puede avanzar de fase: {phase} tiene partidos sin resultado cargado.");
        }
    }

    private static Phase NextPhase(Phase phase)
    {
        return phase switch
        {
            Phase.RoundOf32 => Phase.RoundOf16,
            Phase.RoundOf16 => Phase.QuarterFinals,
            Phase.QuarterFinals => Phase.SemiFinal,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "No hay una fase siguiente definida."),
        };
    }

    public void SimulateUntilFinal(string? performedByEmail)
    {
        const int safetyCap = 20;
        int iterations = 0;

        while (HasPendingMatches() && iterations < safetyCap)
        {
            SimulateNextRound(performedByEmail);
            iterations++;
        }
    }

    private QualifiedTeams ComputeQualifiedTeams(IReadOnlyCollection<Group> groups, TournamentSettings settings)
    {
        List<Match> matches = groups
            .SelectMany(group => _matchRepository.GetByGroup(group.Name) ?? new List<Match>())
            .ToList();

        return new GroupQualifier().ClassifyTeams(groups.ToList(), matches, settings.CrossingsSeed);
    }

    private bool TrySimulatePhase(
        List<MatchElimination> all,
        Phase phase,
        TournamentSettings settings,
        string? performedByEmail)
    {
        List<MatchElimination> phaseMatches = all.Where(match => match.Phase == phase).ToList();
        if (phaseMatches.Count == 0)
        {
            return false;
        }

        List<MatchElimination> unplayed = phaseMatches.Where(match => !IsMatchPlayed(match)).ToList();
        if (unplayed.Count == 0)
        {
            return false;
        }

        SimulateMatches(unplayed, settings);
        CommitAndAuditRound(phase, performedByEmail);
        return true;
    }

    private bool TryCreateAndSimulatePhaseFromPrevious(
        List<MatchElimination> all,
        Phase phase,
        TournamentSettings settings,
        string? performedByEmail)
    {
        List<MatchElimination> existing = all.Where(match => match.Phase == phase).ToList();
        if (existing.Count > 0)
        {
            return false;
        }

        Phase previousPhase = PreviousPhase(phase);
        List<MatchElimination> previous = all.Where(match => match.Phase == previousPhase).ToList();
        if (previous.Count == 0 || previous.Any(match => !IsMatchPlayed(match)))
        {
            return false;
        }

        List<MatchElimination> created = CreateRoundFromWinners(previous, phase, settings);
        foreach (MatchElimination match in created)
        {
            _eliminationRepository.Add(match);
        }

        SimulateMatches(created, settings);
        CommitAndAuditRound(phase, performedByEmail);
        return true;
    }

    private bool TryCreateAndSimulateFinalAndThirdPlace(
        List<MatchElimination> all,
        TournamentSettings settings,
        string? performedByEmail)
    {
        List<MatchElimination> semiFinals = all.Where(match => match.Phase == Phase.SemiFinal).ToList();
        if (semiFinals.Count == 0 || semiFinals.Any(match => !IsMatchPlayed(match)))
        {
            return false;
        }

        List<MatchElimination> existingFinals = all.Where(match => match.Phase == Phase.Final).ToList();
        List<MatchElimination> existingThirds = all.Where(match => match.Phase == Phase.ThirdPlace).ToList();

        if (existingFinals.Count == 0 && existingThirds.Count == 0)
        {
            List<MatchElimination> finalMatches = CreateRoundFromWinners(semiFinals, Phase.Final, settings);
            foreach (MatchElimination match in finalMatches)
            {
                _eliminationRepository.Add(match);
            }

            List<MatchElimination> thirdMatches = CreateRoundFromLosers(semiFinals, Phase.ThirdPlace, settings);
            foreach (MatchElimination match in thirdMatches)
            {
                _eliminationRepository.Add(match);
            }

            SimulateMatches(finalMatches.Concat(thirdMatches).ToList(), settings);
            CommitAndAuditRound(Phase.Final, performedByEmail);
            return true;
        }

        List<MatchElimination> pending = existingFinals.Concat(existingThirds).Where(match => !IsMatchPlayed(match)).ToList();
        if (pending.Count == 0)
        {
            return false;
        }

        SimulateMatches(pending, settings);
        CommitAndAuditRound(Phase.Final, performedByEmail);
        return true;
    }

    private List<MatchElimination> CreateRoundFromWinners(
        List<MatchElimination> previousRound,
        Phase nextPhase,
        TournamentSettings settings)
    {
        int nextId = NextMatchId();
        // Calendario por ronda (igual que BuildRoundOf32): rota estadio y franja horaria por indice
        // de partido para que ningun par comparta (estadio, dia, hora). Antes todos los partidos de
        // la ronda compartian estadio y fecha fijos, colisionando en octavos (8) y cuartos (4).
        KnockoutRoundScheduler scheduler = CreateRoundScheduler(settings, nextPhase);
        List<MatchElimination> created = new();

        // El emparejamiento por adyacencia depende del orden: se ordena por MatchId para que el
        // bracket sea determinista (C1..C8 = ganadores Ai vs Bi, segun el orden intercalado del R32).
        previousRound = previousRound.OrderBy(match => match.MatchId).ToList();

        int matchIndex = 0;
        for (int i = 0; i < previousRound.Count; i += 2)
        {
            MatchElimination sourceHome = previousRound[i];
            MatchElimination sourceAway = previousRound[i + 1];
            Team home = WinnerOf(sourceHome);
            Team away = WinnerOf(sourceAway);
            string code = $"{nextPhase}-{nextId}";

            MatchElimination match = MatchElimination.CreateMatchWithConfig(
                nextId++,
                scheduler.DateProvider(matchIndex),
                home,
                away,
                scheduler.StadiumProvider(matchIndex),
                settings,
                nextPhase,
                code,
                sourceHome,
                sourceAway);

            created.Add(match);
            matchIndex++;
        }

        return created;
    }

    private List<MatchElimination> CreateRoundFromLosers(
        List<MatchElimination> semiFinals,
        Phase phase,
        TournamentSettings settings)
    {
        if (semiFinals.Count != 2)
        {
            return new List<MatchElimination>();
        }

        int nextId = NextMatchId();
        // El tercer puesto es un unico partido: el calendario por ronda le asigna el primer slot
        // (14:00) de la fecha base de la fase, en un estadio segun la rotacion.
        KnockoutRoundScheduler scheduler = CreateRoundScheduler(settings, phase);

        MatchElimination first = semiFinals[0];
        MatchElimination second = semiFinals[1];
        Team home = LoserOf(first);
        Team away = LoserOf(second);

        MatchElimination thirdPlace = MatchElimination.CreateMatchWithConfig(
            nextId,
            scheduler.DateProvider(0),
            home,
            away,
            scheduler.StadiumProvider(0),
            settings,
            phase,
            $"3PL-{nextId}",
            first,
            second);

        return new List<MatchElimination> { thirdPlace };
    }

    private static DateTime KnockoutPhaseDate(TournamentSettings settings, Phase phase)
    {
        return settings.StartDate.AddDays(KnockoutPhaseOffsetDays(phase));
    }

    // Arma el calendario de una ronda usando la fecha base por fase (offset cronologico) como dia
    // de arranque. Mantener el offset por fase evita que rondas distintas se solapen entre si.
    private KnockoutRoundScheduler CreateRoundScheduler(TournamentSettings settings, Phase phase)
    {
        List<Stadium> stadiums = _stadiumRepository.GetAll().ToList();
        if (stadiums.Count == 0)
        {
            throw new KnockoutGeneratorException(StadiumsRequiredMessage);
        }

        return new KnockoutRoundScheduler(stadiums, settings.MaxMatchesPerDay, KnockoutPhaseDate(settings, phase));
    }

    // Offset cronologico explicito (en dias desde StartDate) por fase eliminatoria.
    // El enum Phase NO esta en orden cronologico (Final=0 ... RoundOf32=4), por eso no se
    // puede derivar de (int)phase: hacerlo dejaba la Final antes de las rondas previas (H3).
    // Las separaciones dejan margen para el spread multi-dia del R32 (16 partidos / dia).
    private static int KnockoutPhaseOffsetDays(Phase phase)
    {
        return phase switch
        {
            Phase.RoundOf32 => DaysFromStartToFirstKnockoutRound,
            Phase.RoundOf16 => DaysFromStartToFirstKnockoutRound + 6,
            Phase.QuarterFinals => DaysFromStartToFirstKnockoutRound + 12,
            Phase.SemiFinal => DaysFromStartToFirstKnockoutRound + 18,
            Phase.ThirdPlace => DaysFromStartToFirstKnockoutRound + 22,
            Phase.Final => DaysFromStartToFirstKnockoutRound + 24,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Fase eliminatoria sin offset definido."),
        };
    }

    private static Team WinnerOf(MatchElimination match)
    {
        return match.GetWinner()
            ?? throw new MatchServicesException(string.Format(MatchWithoutWinnerMessage, match.MatchCode));
    }

    private static Team LoserOf(MatchElimination match)
    {
        Team winner = WinnerOf(match);
        return ReferenceEquals(winner, match.HomeTeam) ? match.AwayTeam! : match.HomeTeam!;
    }

    private void SimulateMatches(
        List<MatchElimination> matches,
        TournamentSettings settings)
    {
        IMatchSimulationEngine engine = ResolveSelectedEngine(settings);
        foreach (MatchElimination match in matches)
        {
            int seed = settings.SimulationSeed + match.MatchId;
            engine.Simulate(match, seed);
            ApplyRankingUpdate(match);
            _eliminationRepository.Update(match);
            NotifyJournalistsAboutSimulatedResult(match);
        }
    }

    private void ApplyRankingUpdate(Match match)
    {
        _rankingService.ApplyMatchResultOnce(match);
    }

    private IMatchSimulationEngine ResolveSelectedEngine(TournamentSettings settings)
    {
        return _simulationEngineRegistry.Resolve(settings.SelectedSimulationEngine);
    }

    private void CommitAndAuditRound(Phase phase, string? performedByEmail)
    {
        _auditLog.LogPerformedBy(
            AuditActions.SimulateRound,
            $"Simulated phase {phase}.",
            performedByEmail,
            nameof(KnockoutFixtureGenerator));
    }

    private bool HasPendingMatches()
    {
        List<MatchElimination> all = LoadAllEliminationMatches();
        if (all.Count == 0)
        {
            return false;
        }

        bool finalExists = all.Any(match => match.Phase == Phase.Final);
        bool thirdPlaceExists = all.Any(match => match.Phase == Phase.ThirdPlace);

        if (!finalExists || !thirdPlaceExists)
        {
            return true;
        }

        return all.Any(match => !IsMatchPlayed(match));
    }

    private List<MatchElimination> LoadAllEliminationMatches()
    {
        return _eliminationRepository.GetAll().OfType<MatchElimination>().ToList();
    }

    private TournamentSettings LoadSettingsOrThrow()
    {
        return _settingsRepository.GetAll().FirstOrDefault()
            ?? throw new KnockoutGeneratorException(TournamentSettingsRequiredMessage);
    }

    private static TournamentSettings WithCrossingsSeed(TournamentSettings settings, int crossingsSeed)
    {
        if (settings.CrossingsSeed == crossingsSeed)
        {
            return settings;
        }

        return new TournamentSettings(
            settings.Id,
            settings.StartDate,
            settings.MaxMatchesPerDay,
            settings.DaysBetweenJourneys,
            settings.FixtureSeed,
            crossingsSeed,
            settings.SimulationSeed,
            settings.CompletionSeed);
    }

    private void EnsureBracketNotYetGenerated()
    {
        if (IsBracketGenerated())
        {
            throw new MatchServicesException(BracketAlreadyGeneratedMessage);
        }
    }

    private void EnsureBracketGenerated()
    {
        if (!IsBracketGenerated())
        {
            throw new MatchServicesException(BracketNotGeneratedMessage);
        }
    }

    private static void EnsureTwelveGroups(IReadOnlyCollection<Group> groups)
    {
        if (groups.Count != RequiredGroupCount)
        {
            throw new MatchServicesException(GroupsRequiredMessage);
        }
    }

    private void EnsureGroupStageComplete()
    {
        List<MatchGroup> groupMatches = _matchRepository.GetAll().OfType<MatchGroup>().ToList();
        if (groupMatches.Count != RequiredGroupStageMatchCount || groupMatches.Any(match => !IsMatchPlayed(match)))
        {
            throw new MatchServicesException(GroupStageIncompleteMessage);
        }
    }

    private static bool IsMatchPlayed(Match match)
    {
        return match.HasResult;
    }

    private int NextMatchId()
    {
        int max = _eliminationRepository.GetAll().Select(match => match.MatchId).DefaultIfEmpty(FirstNewMatchIdSeed - 1).Max();
        return max + 1;
    }

    private static Phase PreviousPhase(Phase phase)
    {
        return phase switch
        {
            Phase.RoundOf16 => Phase.RoundOf32,
            Phase.QuarterFinals => Phase.RoundOf16,
            Phase.SemiFinal => Phase.QuarterFinals,
            Phase.Final => Phase.SemiFinal,
            Phase.ThirdPlace => Phase.SemiFinal,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "No hay una fase previa definida."),
        };
    }

    private static void EnsureEveryQualifiedTeamHasGroup(QualifiedTeams qualifiedTeams)
    {
        IEnumerable<Team> qualified = qualifiedTeams.TopEightFirsts
            .Concat(qualifiedTeams.RemainingFourFirsts)
            .Concat(qualifiedTeams.TopEightSeconds)
            .Concat(qualifiedTeams.WorstFourSeconds)
            .Concat(qualifiedTeams.BestEightThirds);

        foreach (Team team in qualified)
        {
            if (!qualifiedTeams.GroupByTeamId.TryGetValue(team.Id, out string? groupName) || string.IsNullOrWhiteSpace(groupName))
            {
                throw new KnockoutGeneratorException($"{MissingGroupForTeamMessagePrefix}{team.Id}.");
            }
        }
    }

    private void NotifyJournalistsAboutSimulatedResult(Match match)
    {
        _notificationService.NotifyMatchSimulated(match);
    }
}
