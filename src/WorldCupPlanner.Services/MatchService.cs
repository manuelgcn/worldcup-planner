using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository matchRepository;
    private readonly ITournamentSettingsRepository tournamentSettingsRepository;
    private readonly IMatchFactory matchFactory;
    private readonly ISimulationEngineRegistry simulationEngineRegistry;
    private readonly IAuditLogService auditLogService;
    private readonly IRankingService rankingService;
    private readonly INotificationService notificationService;
    private readonly IEliminationMatchRepository eliminationMatchRepository;

    private const int DefaultTournamentSettingsId = 1;
    private const string MatchIdAlreadyExistsMessage = "El id de partido ya existe";
    private const string GroupNameRequiredMessage = "El nombre del grupo es obligatorio";
    private const string FromMustBeLessOrEqualToMessage = "'desde' debe ser menor o igual que 'hasta'";
    private const string MatchNotFoundMessage = "Partido no encontrado";
    private const string ValueRequiredMessage = "El valor es obligatorio";
    private const string TournamentSettingsNotFoundMessage = "No se encontro la configuracion del torneo";
    private const string GroupMatchFrozenByKnockoutMessage = "No se pueden editar los partidos de fase de grupos una vez generada la fase eliminatoria.";
    private const string KnockoutMatchFeedsNextRoundMessage = "No se puede editar este partido: la ronda siguiente ya se armo con su resultado.";
    private const string KnockoutDrawMessage = "Un partido de eliminatoria necesita un ganador: no puede quedar empatado.";
    private const string IncidentNotFoundMessage = "Incidencia no encontrada";
    private const string IncidentTeamNotInMatchMessage = "El equipo de la incidencia debe ser uno de los dos que disputan el partido";
    private const string ResultRegisteredDetailTemplate = "MatchId={0} resultado registrado: {1}-{2}.";

    public MatchService(
        IMatchRepository matchRepository,
        ITournamentSettingsRepository tournamentSettingsRepository,
        IMatchFactory matchFactory,
        ISimulationEngineRegistry simulationEngineRegistry,
        IAuditLogService auditLogService,
        IRankingService rankingService,
        INotificationService notificationService,
        IEliminationMatchRepository eliminationMatchRepository)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(eliminationMatchRepository);
        this.matchRepository = matchRepository;
        this.tournamentSettingsRepository = tournamentSettingsRepository;
        this.matchFactory = matchFactory;
        this.simulationEngineRegistry = simulationEngineRegistry;
        this.auditLogService = auditLogService;
        this.rankingService = rankingService;
        this.notificationService = notificationService;
        this.eliminationMatchRepository = eliminationMatchRepository;
    }

    public MatchDto CreateMatch(CreateMatchRequestDto request)
    {
        ValidateRequired(request);

        if (matchRepository.ExistsById(request.MatchId))
        {
            throw new MatchServicesException(MatchIdAlreadyExistsMessage);
        }

        Match match = matchFactory.Create(request);
        matchRepository.Add(match);

        LogAuditAction(
            AuditActions.CreateMatch,
            $"MatchId={match.MatchId} created.",
            match.MatchId,
            request.TournamentSettingsId);


        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);

        matchFactory.ApplyUpdate(match, request);

        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Match updated for MatchId={match.MatchId}.",
            match.MatchId,
            request.TournamentSettingsId);

        matchRepository.Update(match);

        return MapToDto(match);
    }

    public void DeleteMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        matchRepository.Delete(match);

        LogAuditAction(
            AuditActions.DeleteMatch,
            $"MatchId={matchId} deleted.",
            matchId);
    }

    public void AddGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        if (isHomeTeam) match.AddHomeGoal();
        else match.AddAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Goal added to {teamStr} team in MatchId={matchId}.",
            matchId);

        matchRepository.Update(match);
    }

    public void RemoveGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        if (isHomeTeam) match.RemoveHomeGoal();
        else match.RemoveAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Goal removed from {teamStr} team in MatchId={matchId}.",
            matchId);

        matchRepository.Update(match);
    }

    public void PublishResult(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        LogAuditAction(
            AuditActions.PublishMatchResult,
            $"MatchId={matchId} published.",
            matchId);

        ApplyRankingUpdate(match);

        matchRepository.Update(match);
    }

    public void SimulateMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        TournamentSettings tournamentSettings = GetSimulationTournamentSettingsOrThrow();
        IMatchSimulationEngine engine = simulationEngineRegistry.Resolve(tournamentSettings.SelectedSimulationEngine);

        engine.Simulate(match, tournamentSettings.SimulationSeed);
        LogAuditAction(
            AuditActions.SimulationExecuted,
            $"MatchId={matchId} simulated with engine {engine.Name} and seed {tournamentSettings.SimulationSeed}.",
            matchId);

        ApplyRankingUpdate(match);
        notificationService.NotifyMatchSimulated(match);

        matchRepository.Update(match);
    }

    public void AddYellowCard(int matchId, int teamId, int minute, string? performedByEmail)
    {
        AddIncident(matchId, teamId, performedByEmail,
            (id, team) => new YellowCardIncident(id, minute, team));
    }

    public void AddRedCard(int matchId, int teamId, int minute, string? performedByEmail)
    {
        AddIncident(matchId, teamId, performedByEmail,
            (id, team) => new RedCardIncident(id, minute, team));
    }

    public void RemoveIncident(int matchId, int incidentId, string? performedByEmail)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);

        MatchIncident incident = match.Incidents.FirstOrDefault(stored => stored.Id == incidentId)
                                 ?? throw new MatchServicesException(IncidentNotFoundMessage);

        match.RemoveIncident(incident);

        LogIncidentAction(
            AuditActions.IncidentRemoved,
            $"Incident Id={incidentId} ({IncidentTypeName(incident)}) removed from MatchId={matchId}.",
            matchId,
            performedByEmail);

        matchRepository.Update(match);
    }

    public List<MatchIncidentDto> GetIncidents(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        return match.Incidents.Select(MapToIncidentDto).ToList();
    }

    private void AddIncident(int matchId, int teamId, string? performedByEmail, Func<int, Team, MatchIncident> factory)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);

        Team team = ResolveMatchTeamOrThrow(match, teamId);
        MatchIncident incident = factory(matchRepository.NextIncidentId(), team);
        match.AddIncident(incident);

        LogIncidentAction(
            AuditActions.IncidentAdded,
            $"Incident ({IncidentTypeName(incident)}) added to MatchId={matchId} for TeamId={teamId} at minute {incident.Minute}.",
            matchId,
            performedByEmail);

        matchRepository.Update(match);
    }

    // Las incidencias exigen la misma referencia de Team que juega el partido
    // (el dominio compara por referencia), por eso se resuelve desde el Match.
    private static Team ResolveMatchTeamOrThrow(Match match, int teamId)
    {
        if (match.HomeTeam?.Id == teamId)
        {
            return match.HomeTeam;
        }

        if (match.AwayTeam?.Id == teamId)
        {
            return match.AwayTeam;
        }

        throw new MatchServicesException(IncidentTeamNotInMatchMessage);
    }

    private void LogIncidentAction(string action, string details, int matchId, string? performedByEmail)
    {
        auditLogService.LogPerformedBy(
            action: action,
            details: details,
            performedByEmail: performedByEmail,
            affectedEntityType: nameof(Match),
            affectedEntityId: matchId);
    }

    private static string IncidentTypeName(MatchIncident incident)
    {
        return incident is RedCardIncident ? MatchIncidentDto.RedCardType : MatchIncidentDto.YellowCardType;
    }

    private static MatchIncidentDto MapToIncidentDto(MatchIncident incident)
    {
        return new MatchIncidentDto
        {
            Id = incident.Id,
            Minute = incident.Minute,
            TeamId = incident.Team.Id,
            TeamName = incident.Team.Name,
            Type = IncidentTypeName(incident)
        };
    }

    public MatchDto GetById(int matchId)
    {
        return MapToDto(GetMatchOrThrow(matchId));
    }

    public Match GetEntity(int matchId)
    {
        return GetMatchOrThrow(matchId);
    }

    public IReadOnlySet<int> GetLockedKnockoutMatchIds()
    {
        return eliminationMatchRepository.GetAll()
            .OfType<MatchElimination>()
            .SelectMany(next => new[] { next.MatchHome?.MatchId, next.MatchAway?.MatchId })
            .OfType<int>()
            .ToHashSet();
    }

    public List<MatchDto> GetByGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new MatchServicesException(GroupNameRequiredMessage);
        }

        return matchRepository.GetByGroup(groupName).Select(MapToDto).ToList();
    }

    public List<MatchDto> GetByJourney(int journeyNumber)
    {
        return matchRepository.GetByJourney(journeyNumber).Select(MapToDto).ToList();
    }

    public List<MatchDto> GetByDateRange(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new MatchServicesException(FromMustBeLessOrEqualToMessage);
        }

        return matchRepository.GetByDateRange(from, to).Select(MapToDto).ToList();
    }

    public void SetGoals(int matchId, int homeGoals, int awayGoals)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        EnsureKnockoutResultHasWinner(match, homeGoals, awayGoals);
        match.SetGoals(homeGoals, awayGoals);
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"MatchId={matchId} goals set to {homeGoals}-{awayGoals}.",
            matchId);
        matchRepository.Update(match);
    }

    // Controlador GRASP del caso de uso "registrar resultado": carga el partido, valida que
    // sea editable, registra los goles, audita con el actor, aplica el ranking una sola vez,
    // notifica a periodistas y persiste. La UI solo captura datos y delega aca.
    public MatchDto RegisterResult(int matchId, int homeGoals, int awayGoals, string? performedByEmail)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureMatchIsEditable(match);
        EnsureKnockoutResultHasWinner(match, homeGoals, awayGoals);

        int previousHomeGoals = match.HomeGoals;
        int previousAwayGoals = match.AwayGoals;

        match.SetGoals(homeGoals, awayGoals);
        LogResultRegistered(matchId, homeGoals, awayGoals, performedByEmail);
        // Editar un resultado ya registrado debe recalcular el Elo (revertir el anterior y aplicar
        // el nuevo), no quedar bloqueado por el flag de "aplicado una vez" (H7).
        rankingService.ReapplyMatchResult(match);
        notificationService.NotifyResultLoaded(match, previousHomeGoals, previousAwayGoals);

        matchRepository.Update(match);

        return MapToDto(match);
    }

    private void LogResultRegistered(int matchId, int homeGoals, int awayGoals, string? performedByEmail)
    {
        auditLogService.LogPerformedBy(
            action: AuditActions.UpdateMatch,
            details: string.Format(ResultRegisteredDetailTemplate, matchId, homeGoals, awayGoals),
            performedByEmail: performedByEmail,
            affectedEntityType: nameof(Match),
            affectedEntityId: matchId);
    }

    private void ApplyRankingUpdate(Match match)
    {
        rankingService.ApplyMatchResultOnce(match);
    }

    private void LogAuditAction(string action, string details, int affectedEntityId, int? tournamentSettingsId = null)
    {
        if (tournamentSettingsId.HasValue)
        {
            auditLogService.Log(
                action: action,
                details: details,
                tournamentSettingsId: tournamentSettingsId.Value,
                affectedEntityType: nameof(Match),
                affectedEntityId: affectedEntityId);
            return;
        }

        auditLogService.Log(
            action: action,
            details: details,
            affectedEntityType: nameof(Match),
            affectedEntityId: affectedEntityId);
    }

    private MatchDto MapToDto(Match match)
    {
        return new MatchDto
        {
            MatchId = match.MatchId,
            MatchDate = match.MatchDate,
            HomeGoals = match.HomeGoals,
            AwayGoals = match.AwayGoals
        };
    }

    private Match GetMatchOrThrow(int matchId)
    {
        return matchRepository.GetById(matchId) ?? throw new MatchServicesException(MatchNotFoundMessage);
    }

    // Un partido de grupos se congela al generarse la eliminatoria; uno de eliminatoria, cuando
    // la ronda siguiente ya tomo su ganador (editarlo dejaria al equipo equivocado en el cuadro).
    private void EnsureMatchIsEditable(Match match)
    {
        if (match is MatchGroup)
        {
            if (eliminationMatchRepository.Any())
            {
                throw new MatchServicesException(GroupMatchFrozenByKnockoutMessage);
            }

            return;
        }

        if (GetLockedKnockoutMatchIds().Contains(match.MatchId))
        {
            throw new MatchServicesException(KnockoutMatchFeedsNextRoundMessage);
        }
    }

    // La carga manual no tiene penales: un empate solo vale si es el mismo marcador que ya se
    // definio por penales (p. ej. al guardar otra vez un partido simulado sin cambiar el resultado).
    private static void EnsureKnockoutResultHasWinner(Match match, int homeGoals, int awayGoals)
    {
        if (match is not MatchElimination elimination || homeGoals != awayGoals)
        {
            return;
        }

        bool keepsPenaltyDecision = elimination.WonByPenalties
            && elimination.HomeGoals == homeGoals
            && elimination.AwayGoals == awayGoals;
        if (!keepsPenaltyDecision)
        {
            throw new MatchServicesException(KnockoutDrawMessage);
        }
    }

    private TournamentSettings GetSimulationTournamentSettingsOrThrow()
    {
        return tournamentSettingsRepository.GetById(DefaultTournamentSettingsId)
               ?? throw new MatchServicesException(TournamentSettingsNotFoundMessage);
    }
    
    private static void ValidateRequired(object? value)
    {
        if (value == null)
        {
            throw new MatchServicesException(ValueRequiredMessage);
        }
    }
}
