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
    IMatchSimulationEngine matchSimulationEngine,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork)
    : IMatchService
{
    private const int SimulationTournamentSettingsId = 1;
    private const string MatchIdAlreadyExistsMessage = "Match ID already exists";
    private const string GroupNameRequiredMessage = "Group name is required";
    private const string FromMustBeLessOrEqualToMessage = "'from' must be less than or equal to 'to'";
    private const string MatchNotFoundMessage = "Match not found";
    private const string ValueRequiredMessage = "Value is required";
    private const string TournamentSettingsNotFoundMessage = "Tournament settings not found";

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
            request.TournamentSettingsId ?? 1, 
            match.MatchId);
            
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        Match match = GetMatchOrThrow(matchId);

        matchFactory.ApplyUpdate(match, request);
        
        LogAuditAction(
            AuditActions.UpdateMatch, 
            $"Match updated for MatchId={match.MatchId}.", 
            SimulationTournamentSettingsId,
            match.MatchId);

        matchRepository.Update(match);
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public void DeleteMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        matchRepository.Delete(match);
        
        LogAuditAction(
            AuditActions.DeleteMatch, 
            $"MatchId={matchId} deleted.", 
            1, 
            matchId);
            
        unitOfWork.Commit();
    }

    public void AddGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        if (isHomeTeam) match.AddHomeGoal();
        else match.AddAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch, 
            $"Goal added to {teamStr} team in MatchId={matchId}.", 
            SimulationTournamentSettingsId, 
            matchId);

        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public void RemoveGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);
        if (isHomeTeam) match.RemoveHomeGoal();
        else match.RemoveAwayGoal();

        string teamStr = isHomeTeam ? "Home" : "Away";
        LogAuditAction(
            AuditActions.UpdateMatch, 
            $"Goal removed from {teamStr} team in MatchId={matchId}.", 
            SimulationTournamentSettingsId, 
            matchId);

        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public void PublishResult(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public void SimulateMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        TournamentSettings tournamentSettings = GetSimulationTournamentSettingsOrThrow();

        matchSimulationEngine.Simulate(match, tournamentSettings.SimulationSeed);
        matchRepository.Update(match);
        unitOfWork.Commit();
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
        match.SetGoals(homeGoals, awayGoals);
        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    private void LogAuditAction(string action, string details, int tournamentSettingsId, int affectedEntityId)
    {
        auditLogService.Log(
            action: action,
            details: details,
            tournamentSettingsId: tournamentSettingsId,
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

    private TournamentSettings GetSimulationTournamentSettingsOrThrow()
    {
        return tournamentSettingsRepository.GetById(SimulationTournamentSettingsId)
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