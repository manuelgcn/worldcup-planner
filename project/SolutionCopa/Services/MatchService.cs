using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class MatchService(
    IMatchRepository matchRepository,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IMatchFactory matchFactory,
    ISimulationEngineRegistry simulationEngineRegistry,
    IAuditLogService auditLogService,
    IRankingService rankingService,
    IUnitOfWork unitOfWork,
    IEliminationMatchRepository? eliminationMatchRepository = null)
    : IMatchService
{
    private const int DefaultTournamentSettingsId = 1;
    private const string MatchIdAlreadyExistsMessage = "Match ID already exists";
    private const string GroupNameRequiredMessage = "Group name is required";
    private const string FromMustBeLessOrEqualToMessage = "'from' must be less than or equal to 'to'";
    private const string MatchNotFoundMessage = "Match not found";
    private const string ValueRequiredMessage = "Value is required";
    private const string TournamentSettingsNotFoundMessage = "Tournament settings not found";
    private const string GroupMatchFrozenByKnockoutMessage = "Group-stage matches cannot be edited once the knockout phase is generated.";
    private const string IncidentNotFoundMessage = "Incident not found";
    private const string IncidentTeamNotInMatchMessage = "Incident team must be one of the two teams playing the match";

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

        unitOfWork.Commit();

        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);

        matchFactory.ApplyUpdate(match, request);

        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Match updated for MatchId={match.MatchId}.",
            match.MatchId,
            request.TournamentSettingsId);

        matchRepository.Update(match);
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public void DeleteMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);
        matchRepository.Delete(match);

        LogAuditAction(
            AuditActions.DeleteMatch,
            $"MatchId={matchId} deleted.",
            matchId);

        unitOfWork.Commit();
    }

    public void AddGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);
        if (isHomeTeam) match.AddHomeGoal();
        else match.AddAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Goal added to {teamStr} team in MatchId={matchId}.",
            matchId);

        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public void RemoveGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);
        if (isHomeTeam) match.RemoveHomeGoal();
        else match.RemoveAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"Goal removed from {teamStr} team in MatchId={matchId}.",
            matchId);

        matchRepository.Update(match);
        unitOfWork.Commit();
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
        unitOfWork.Commit();
    }

    public void SimulateMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);
        TournamentSettings tournamentSettings = GetSimulationTournamentSettingsOrThrow();
        IMatchSimulationEngine engine = simulationEngineRegistry.Resolve(tournamentSettings.SelectedSimulationEngine);

        engine.Simulate(match, tournamentSettings.SimulationSeed);
        LogAuditAction(
            AuditActions.SimulationExecuted,
            $"MatchId={matchId} simulated with engine {engine.Name} and seed {tournamentSettings.SimulationSeed}.",
            matchId);

        ApplyRankingUpdate(match);

        matchRepository.Update(match);
        unitOfWork.Commit();
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
        EnsureGroupMatchIsEditable(match);

        MatchIncident incident = match.Incidents.FirstOrDefault(stored => stored.Id == incidentId)
                                 ?? throw new MatchServicesException(IncidentNotFoundMessage);

        match.RemoveIncident(incident);

        LogIncidentAction(
            AuditActions.IncidentRemoved,
            $"Incident Id={incidentId} ({IncidentTypeName(incident)}) removed from MatchId={matchId}.",
            matchId,
            performedByEmail);

        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public List<MatchIncidentDto> GetIncidents(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        return match.Incidents.Select(MapToIncidentDto).ToList();
    }

    private void AddIncident(int matchId, int teamId, string? performedByEmail, Func<int, Team, MatchIncident> factory)
    {
        Match match = GetMatchOrThrow(matchId);
        EnsureGroupMatchIsEditable(match);

        Team team = ResolveMatchTeamOrThrow(match, teamId);
        MatchIncident incident = factory(matchRepository.NextIncidentId(), team);
        match.AddIncident(incident);

        LogIncidentAction(
            AuditActions.IncidentAdded,
            $"Incident ({IncidentTypeName(incident)}) added to MatchId={matchId} for TeamId={teamId} at minute {incident.Minute}.",
            matchId,
            performedByEmail);

        matchRepository.Update(match);
        unitOfWork.Commit();
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
        EnsureGroupMatchIsEditable(match);
        match.SetGoals(homeGoals, awayGoals);
        LogAuditAction(
            AuditActions.UpdateMatch,
            $"MatchId={matchId} goals set to {homeGoals}-{awayGoals}.",
            matchId);
        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    private void ApplyRankingUpdate(Match match)
    {
        MatchPhase phase = match is MatchGroup ? MatchPhase.GroupStage : MatchPhase.Knockout;
        rankingService.ApplyMatchResult(match, phase);
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

    private void EnsureGroupMatchIsEditable(Match match)
    {
        if (match is not MatchGroup)
        {
            return;
        }

        if (eliminationMatchRepository == null)
        {
            return;
        }

        if (eliminationMatchRepository.GetAll().Count > 0)
        {
            throw new MatchServicesException(GroupMatchFrozenByKnockoutMessage);
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
