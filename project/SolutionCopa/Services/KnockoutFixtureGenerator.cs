using DataAccess.Interfaces;
using ProjectCopa;
using ProjectCopa.Models;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class KnockoutFixtureGenerator : IKnockoutFixtureGenerator
{
    private const int RequiredGroupCount = 12;
    private const int RequiredGroupStageMatchCount = 72;
    private const int FirstNewMatchIdSeed = 3000;
    private const int DaysFromStartToFirstKnockoutRound = 60;
    private const string TournamentSettingsRequiredMessage = "La configuracion del torneo es obligatoria para generar la fase 2.";
    private const string StadiumsRequiredMessage = "Se requiere al menos un estadio para generar la fase 2.";
    private const string GroupsRequiredMessage = "Deben existir los 12 grupos antes de generar el cuadro eliminatorio.";
    private const string GroupStageIncompleteMessage = "La fase de grupos no esta completa: cada partido debe tener al menos un gol registrado.";
    private const string BracketAlreadyGeneratedMessage = "El cuadro eliminatorio ya fue generado.";
    private const string BracketNotGeneratedMessage = "El cuadro eliminatorio aun no fue generado.";
    private const string MissingGroupForTeamMessagePrefix = "No se encontro grupo para el equipo ";

    private readonly ISimulationEngineRegistry _simulationEngineRegistry;
    private readonly IMatchRepository _matchRepository;
    private readonly IEliminationMatchRepository _eliminationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
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
        IUnitOfWork unitOfWork,
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
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(auditLog);
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(rankingService);
        _simulationEngineRegistry = simulationEngineRegistry;
        _matchRepository = matchRepository;
        _eliminationRepository = eliminationRepository;
        _groupRepository = groupRepository;
        _stadiumRepository = stadiumRepository;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _notificationService = notificationService;
        _rankingService = rankingService;
    }

    public List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams)
    {
        ArgumentNullException.ThrowIfNull(qualifiedTeams);

        TournamentSettings settings = LoadSettingsOrThrow();
        List<Stadium> stadiums = _stadiumRepository.GetAll().ToList();
        if (stadiums.Count == 0)
        {
            throw new KnockoutGeneratorException(StadiumsRequiredMessage);
        }

        List<(Team Team, string Group)> qualifiedWithGroups = FlattenQualifiedTeams(qualifiedTeams);

        EliminationBuilder builder = new EliminationBuilder();
        int maxPerDay = settings.MaxMatchesPerDay;
        DateTime roundStart = settings.StartDate;
        Func<int, Stadium> stadiumProvider = matchIndex => stadiums[matchIndex % stadiums.Count];
        Func<int, DateTime> dateProvider = matchIndex => roundStart.AddDays(matchIndex / maxPerDay);

        List<MatchElimination> roundOf32 = builder.CreateRoundOf32(
            qualifiedWithGroups,
            stadiums,
            settings,
            settings.CrossingsSeed,
            stadiumProvider,
            dateProvider);

        return roundOf32.Cast<Match>().ToList();
    }

    public bool IsBracketGenerated()
    {
        return _eliminationRepository != null && _eliminationRepository.GetAll().Count > 0;
    }

    public void GenerateBracket(int? crossingsSeedOverride, string? performedByEmail)
    {
        IEliminationMatchRepository elimRepo = RequireEliminationRepository();
        IGroupRepository groupRepo = RequireGroupRepository();

        EnsureBracketNotYetGenerated();

        TournamentSettings settings = LoadSettingsOrThrow();
        IReadOnlyCollection<Group> groups = groupRepo.GetAll();
        EnsureTwelveGroups(groups);
        EnsureGroupStageComplete();

        int effectiveSeed = crossingsSeedOverride ?? settings.CrossingsSeed;
        TournamentSettings settingsForBracket = WithCrossingsSeed(settings, effectiveSeed);

        QualifiedTeams qualifiedTeams = ComputeQualifiedTeams(groups, settingsForBracket);
        List<Match> roundOf32 = GeneratePhase2(qualifiedTeams);

        foreach (Match match in roundOf32)
        {
            elimRepo.Add(match);
        }

        _unitOfWork.Commit();

        _auditLog.LogPerformedBy(
            AuditActions.GenerateKnockout,
            $"Knockout bracket generated with seed {effectiveSeed}.",
            performedByEmail,
            nameof(KnockoutFixtureGenerator));
    }

    public void SimulateNextRound(string? performedByEmail)
    {
        IEliminationMatchRepository elimRepo = RequireEliminationRepository();
        EnsureBracketGenerated();
        TournamentSettings settings = LoadSettingsOrThrow();

        List<MatchElimination> all = LoadAllEliminationMatches(elimRepo);

        if (TrySimulatePhase(all, Phase.RoundOf32, settings, elimRepo, performedByEmail))
        {
            return;
        }

        Phase[] phasesAfterR32 = { Phase.RoundOf16, Phase.QuarterFinals, Phase.SemiFinal };
        foreach (Phase phase in phasesAfterR32)
        {
            if (TryCreateAndSimulatePhaseFromPrevious(all, phase, settings, elimRepo, performedByEmail))
            {
                return;
            }

            all = LoadAllEliminationMatches(elimRepo);

            if (TrySimulatePhase(all, phase, settings, elimRepo, performedByEmail))
            {
                return;
            }
        }

        TryCreateAndSimulateFinalAndThirdPlace(all, settings, elimRepo, performedByEmail);
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

    public void SimulateEliminationMatches(IEnumerable<int> matchIds)
    {
        TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
        if (settings == null)
        {
            throw new KnockoutGeneratorException("La configuracion del torneo es obligatoria para simular partidos.");
        }

        int baseSeed = settings.SimulationSeed;
        IMatchSimulationEngine engine = ResolveSelectedEngine(settings);

        foreach (int matchId in matchIds)
        {
            Match? match = GetMatchById(matchId);
            if (match == null)
            {
                continue;
            }

            int seed = baseSeed + matchId;
            engine.Simulate(match, seed);
            ApplyRankingUpdate(match);

            _matchRepository.Update(match);
            NotifyJournalistsAboutSimulatedResult(match);
            _auditLog.Log(AuditActions.SimulationExecuted, BuildSimulationAuditDetails(match, seed, engine.Name), settings.Id, "Match", match.MatchId);
        }

        _unitOfWork.Commit();
    }

    private static string BuildSimulationAuditDetails(Match match, int seed, string engineName)
    {
        string baseMessage = $"Simulated elimination match {match.MatchId} with engine {engineName} and seed {seed}.";
        if (match is MatchElimination eliminationMatch && eliminationMatch.WonByPenalties)
        {
            return baseMessage + $" Tiebreaker resolved by penalty shootout {eliminationMatch.HomePenaltyGoals}-{eliminationMatch.AwayPenaltyGoals}.";
        }

        return baseMessage;
    }

    public void OrchestrateAndSimulateEliminationPhase(
        IEnumerable<(Team Team, string Group)> qualifiedWithGroups,
        IReadOnlyCollection<Stadium> stadiums,
        TournamentSettings settings,
        IEliminationMatchRepository eliminationRepo,
        int? crossingsSeedOverride = null)
    {
        if (qualifiedWithGroups == null) throw new ArgumentNullException(nameof(qualifiedWithGroups));

        List<(Team Team, string Group)> entries = qualifiedWithGroups.ToList();
        if (entries.Count != 32) throw new ArgumentException("Requires 32 qualified teams with group info");

        EliminationOrchestrator orchestrator = new EliminationOrchestrator(
            eliminationRepo,
            _auditLog);

        orchestrator.OrchestrateEliminationPhase(entries, stadiums, settings, crossingsSeedOverride);

        IMatchSimulationEngine engine = ResolveSelectedEngine(settings);
        List<Match> allMatches = eliminationRepo.GetAll().ToList();
        foreach (Match match in allMatches)
        {
            try
            {
                int seed = settings.SimulationSeed + match.MatchId;
                engine.Simulate(match, seed);
                ApplyRankingUpdate(match);
                eliminationRepo.Update(match);
                NotifyJournalistsAboutSimulatedResult(match);
                _auditLog.Log(AuditActions.SimulationExecuted, BuildSimulationAuditDetails(match, seed, engine.Name), settings.Id, "Match", match.MatchId);
            }
            catch (Exception ex)
            {
                _auditLog.Log(AuditActions.SimulateMatch, $"Failed to simulate match {match.MatchId}: {ex.Message}", settings.Id, "Match", match.MatchId);
            }
        }

        _unitOfWork.Commit();
    }

    public List<(Team Team, string Group)> ComputeQualified32FromGroups(IReadOnlyCollection<Group> groups, TournamentSettings settings)
    {
        if (groups == null) throw new ArgumentNullException(nameof(groups));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        List<Match> matches = groups
            .SelectMany(group => _matchRepository.GetByGroup(group.Name) ?? new List<Match>())
            .ToList();

        GroupQualifier qualifier = new GroupQualifier();
        QualifiedTeams qualifiedTeams = qualifier.ClassifyTeams(groups.ToList(), matches, settings.CrossingsSeed);

        return FlattenQualifiedTeams(qualifiedTeams);
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
        IEliminationMatchRepository elimRepo,
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

        SimulateMatches(unplayed, settings, elimRepo);
        CommitAndAuditRound(phase, performedByEmail);
        return true;
    }

    private bool TryCreateAndSimulatePhaseFromPrevious(
        List<MatchElimination> all,
        Phase phase,
        TournamentSettings settings,
        IEliminationMatchRepository elimRepo,
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

        List<MatchElimination> created = CreateRoundFromWinners(previous, phase, settings, elimRepo);
        foreach (MatchElimination match in created)
        {
            elimRepo.Add(match);
        }

        SimulateMatches(created, settings, elimRepo);
        CommitAndAuditRound(phase, performedByEmail);
        return true;
    }

    private bool TryCreateAndSimulateFinalAndThirdPlace(
        List<MatchElimination> all,
        TournamentSettings settings,
        IEliminationMatchRepository elimRepo,
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
            List<MatchElimination> finalMatches = CreateRoundFromWinners(semiFinals, Phase.Final, settings, elimRepo);
            foreach (MatchElimination match in finalMatches)
            {
                elimRepo.Add(match);
            }

            List<MatchElimination> thirdMatches = CreateRoundFromLosers(semiFinals, Phase.ThirdPlace, settings, elimRepo);
            foreach (MatchElimination match in thirdMatches)
            {
                elimRepo.Add(match);
            }

            SimulateMatches(finalMatches.Concat(thirdMatches).ToList(), settings, elimRepo);
            CommitAndAuditRound(Phase.Final, performedByEmail);
            return true;
        }

        List<MatchElimination> pending = existingFinals.Concat(existingThirds).Where(match => !IsMatchPlayed(match)).ToList();
        if (pending.Count == 0)
        {
            return false;
        }

        SimulateMatches(pending, settings, elimRepo);
        CommitAndAuditRound(Phase.Final, performedByEmail);
        return true;
    }

    private List<MatchElimination> CreateRoundFromWinners(
        List<MatchElimination> previousRound,
        Phase nextPhase,
        TournamentSettings settings,
        IEliminationMatchRepository elimRepo)
    {
        Stadium stadium = ResolveStadium();
        int nextId = NextMatchId(elimRepo);
        DateTime nextDate = KnockoutPhaseDate(settings, nextPhase);
        List<MatchElimination> created = new();

        for (int i = 0; i < previousRound.Count; i += 2)
        {
            MatchElimination sourceHome = previousRound[i];
            MatchElimination sourceAway = previousRound[i + 1];
            Team home = sourceHome.GetWinner() ?? sourceHome.HomeTeam!;
            Team away = sourceAway.GetWinner() ?? sourceAway.HomeTeam!;
            string code = $"{nextPhase}-{nextId}";

            MatchElimination match = MatchElimination.CreateMatchWithConfig(
                nextId++,
                nextDate,
                home,
                away,
                stadium,
                settings,
                nextPhase,
                code,
                sourceHome,
                sourceAway);

            created.Add(match);
        }

        return created;
    }

    private List<MatchElimination> CreateRoundFromLosers(
        List<MatchElimination> semiFinals,
        Phase phase,
        TournamentSettings settings,
        IEliminationMatchRepository elimRepo)
    {
        if (semiFinals.Count != 2)
        {
            return new List<MatchElimination>();
        }

        Stadium stadium = ResolveStadium();
        int nextId = NextMatchId(elimRepo);
        DateTime nextDate = KnockoutPhaseDate(settings, phase);

        MatchElimination first = semiFinals[0];
        MatchElimination second = semiFinals[1];
        Team home = LoserOf(first);
        Team away = LoserOf(second);

        MatchElimination thirdPlace = MatchElimination.CreateMatchWithConfig(
            nextId,
            nextDate,
            home,
            away,
            stadium,
            settings,
            phase,
            $"3PL-{nextId}",
            first,
            second);

        return new List<MatchElimination> { thirdPlace };
    }

    private static DateTime KnockoutPhaseDate(TournamentSettings settings, Phase phase)
    {
        return settings.StartDate.AddDays(DaysFromStartToFirstKnockoutRound + (int)phase);
    }

    private static Team LoserOf(MatchElimination match)
    {
        Team? winner = match.GetWinner();
        if (winner == null)
        {
            return match.AwayTeam!;
        }

        return ReferenceEquals(winner, match.HomeTeam) ? match.AwayTeam! : match.HomeTeam!;
    }

    private void SimulateMatches(
        List<MatchElimination> matches,
        TournamentSettings settings,
        IEliminationMatchRepository elimRepo)
    {
        IMatchSimulationEngine engine = ResolveSelectedEngine(settings);
        foreach (MatchElimination match in matches)
        {
            int seed = settings.SimulationSeed + match.MatchId;
            engine.Simulate(match, seed);
            ApplyRankingUpdate(match);
            elimRepo.Update(match);
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
        _unitOfWork.Commit();
        _auditLog.LogPerformedBy(
            AuditActions.SimulateRound,
            $"Simulated phase {phase}.",
            performedByEmail,
            nameof(KnockoutFixtureGenerator));
    }

    private bool HasPendingMatches()
    {
        List<MatchElimination> all = LoadAllEliminationMatches(_eliminationRepository);
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

    private static List<MatchElimination> LoadAllEliminationMatches(IEliminationMatchRepository elimRepo)
    {
        return elimRepo.GetAll().OfType<MatchElimination>().ToList();
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

    private IEliminationMatchRepository RequireEliminationRepository()
    {
        return _eliminationRepository;
    }

    private IGroupRepository RequireGroupRepository()
    {
        return _groupRepository;
    }

    private Stadium ResolveStadium()
    {
        Stadium? stadium = _stadiumRepository.GetAll().FirstOrDefault();
        return stadium ?? throw new KnockoutGeneratorException(StadiumsRequiredMessage);
    }

    private int NextMatchId(IEliminationMatchRepository elimRepo)
    {
        int max = elimRepo.GetAll().Select(match => match.MatchId).DefaultIfEmpty(FirstNewMatchIdSeed - 1).Max();
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

    private static List<(Team Team, string Group)> FlattenQualifiedTeams(QualifiedTeams qualifiedTeams)
    {
        List<Team> orderedTeams = qualifiedTeams.TopEightFirsts
            .Concat(qualifiedTeams.RemainingFourFirsts)
            .Concat(qualifiedTeams.TopEightSeconds)
            .Concat(qualifiedTeams.WorstFourSeconds)
            .Concat(qualifiedTeams.BestEightThirds)
            .ToList();

        List<(Team Team, string Group)> qualifiedWithGroups = new(orderedTeams.Count);

        foreach (Team team in orderedTeams)
        {
            if (!qualifiedTeams.GroupByTeamId.TryGetValue(team.Id, out string? groupName) || string.IsNullOrWhiteSpace(groupName))
            {
                throw new KnockoutGeneratorException($"{MissingGroupForTeamMessagePrefix}{team.Id}.");
            }

            qualifiedWithGroups.Add((team, groupName));
        }

        return qualifiedWithGroups;
    }

    private Match? GetMatchById(int matchId)
    {
        try
        {
            return _matchRepository.GetById(matchId);
        }
        catch
        {
            return null;
        }
    }

    private void NotifyJournalistsAboutSimulatedResult(Match match)
    {
        _notificationService.NotifyMatchSimulated(match);
    }
}
