using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class MatchService(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IStadiumRepository stadiumRepository,
    IGroupRepository groupRepository,
    IJourneyRepository journeyRepository,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IMatchSimulationEngine matchSimulationEngine,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork)
    : IMatchService
{
    private const int SimulationTournamentSettingsId = 1;
    private const string MatchIdAlreadyExistsMessage = "Match ID already exists";
    private const string GroupNameRequiredMessage = "Group name is required";
    private const string FromMustBeLessOrEqualToMessage = "'from' must be less than or equal to 'to'";
    private const string PhaseRequiredMessage = "Phase is required";
    private const string MatchCodeRequiredMessage = "MatchCode is required";
    private const string MatchHomeIdRequiredMessage = "MatchHomeId is required";
    private const string MatchAwayIdRequiredMessage = "MatchAwayId is required";
    private const string GroupIdRequiredMessage = "GroupId is required";
    private const string JourneyIdRequiredMessage = "JourneyId is required";
    private const string MatchNotFoundMessage = "Match not found";
    private const string TeamNotFoundMessage = "Team not found";
    private const string StadiumNotFoundMessage = "Stadium not found";
    private const string GroupNotFoundMessage = "Group not found";
    private const string JourneyNotFoundMessage = "Journey not found";
    private const string TournamentSettingsNotFoundMessage = "Tournament settings not found";
    private const string ValueRequiredMessage = "Value is required";

    public MatchDto CreateMatch(CreateMatchRequestDto request)
    {
        ValidateRequired(request);

        if (matchRepository.ExistsById(request.MatchId))
        {
            throw new MatchServicesException(MatchIdAlreadyExistsMessage);
        }

        Match match = BuildMatch(request);
        matchRepository.Add(match);
        auditLogService.Log(
            action: AuditActions.CreateMatch,
            details: $"MatchId={match.MatchId}",
            tournamentSettingsId: request.TournamentSettingsId ?? 1,
            affectedEntityType: nameof(Match),
            affectedEntityId: match.MatchId);
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        Match match = GetMatchOrThrow(matchId);

        if (request.StadiumId.HasValue)
        {
            Stadium stadium = GetStadiumOrThrow(request.StadiumId.Value);
            match.ChangeStadium(stadium);
        }

        matchRepository.Update(match);
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public void DeleteMatch(int matchId)
    {
        Match match = GetMatchOrThrow(matchId);
        matchRepository.Delete(match);
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

    public void AddGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);

        if (isHomeTeam)
        {
            match.AddHomeGoal();
        }
        else
        {
            match.AddAwayGoal();
        }

        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    public void RemoveGoal(int matchId, bool isHomeTeam)
    {
        Match match = GetMatchOrThrow(matchId);

        if (isHomeTeam)
        {
            match.RemoveHomeGoal();
        }
        else
        {
            match.RemoveAwayGoal();
        }

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

    private Match BuildMatch(CreateMatchRequestDto request)
    {
        Team homeTeam = GetTeamOrThrow(request.HomeTeamId);
        Team awayTeam = GetTeamOrThrow(request.AwayTeamId);
        Stadium stadium = GetStadiumOrThrow(request.StadiumId);
        TournamentSettings? tournamentSettings = GetTournamentSettingsOrNull(request.TournamentSettingsId);

        if (request.IsElimination)
        {
            Phase phase = request.Phase ?? throw new MatchServicesException(PhaseRequiredMessage);
            string matchCode = request.MatchCode ?? throw new MatchServicesException(MatchCodeRequiredMessage);
            int sourceHomeId = request.MatchHomeId ?? throw new MatchServicesException(MatchHomeIdRequiredMessage);
            int sourceAwayId = request.MatchAwayId ?? throw new MatchServicesException(MatchAwayIdRequiredMessage);
            Match sourceHome = GetMatchOrThrow(sourceHomeId);
            Match sourceAway = GetMatchOrThrow(sourceAwayId);

            if (tournamentSettings == null)
            {
                return MatchElimination.Create(
                    request.MatchId,
                    request.MatchDate,
                    homeTeam,
                    awayTeam,
                    stadium,
                    phase,
                    matchCode,
                    sourceHome,
                    sourceAway);
            }

            return MatchElimination.CreateMatchWithConfig(
                request.MatchId,
                request.MatchDate,
                homeTeam,
                awayTeam,
                stadium,
                tournamentSettings,
                phase,
                matchCode,
                sourceHome,
                sourceAway);
        }

        int groupId = request.GroupId ?? throw new MatchServicesException(GroupIdRequiredMessage);
        int journeyId = request.JourneyId ?? throw new MatchServicesException(JourneyIdRequiredMessage);
        Group group = GetGroupOrThrow(groupId);
        Journey journey = GetJourneyOrThrow(journeyId);

        if (tournamentSettings == null)
        {
            return MatchGroup.CreateInstance(request.MatchId,
                request.MatchDate,
                homeTeam,
                awayTeam,
                stadium,
                group,
                journey);
        }

        return MatchGroup.CreateMatchWithConfig(request.MatchId,
            request.MatchDate,
            homeTeam,
            awayTeam,
            stadium,
            tournamentSettings,
            group,
            journey);
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

    private Team GetTeamOrThrow(int teamId)
    {
        return teamRepository.GetById(teamId) ?? throw new MatchServicesException(TeamNotFoundMessage);
    }

    private Stadium GetStadiumOrThrow(int stadiumId)
    {
        return stadiumRepository.GetById(stadiumId) ?? throw new MatchServicesException(StadiumNotFoundMessage);
    }

    private Group GetGroupOrThrow(int groupId)
    {
        return groupRepository.GetById(groupId) ?? throw new MatchServicesException(GroupNotFoundMessage);
    }

    private Journey GetJourneyOrThrow(int journeyId)
    {
        return journeyRepository.GetById(journeyId) ?? throw new MatchServicesException(JourneyNotFoundMessage);
    }

    private TournamentSettings? GetTournamentSettingsOrNull(int? tournamentSettingsId)
    {
        if (!tournamentSettingsId.HasValue)
        {
            return null;
        }

        return GetTournamentSettingsOrThrow(tournamentSettingsId.Value);
    }

    private TournamentSettings GetSimulationTournamentSettingsOrThrow()
    {
        return GetTournamentSettingsOrThrow(SimulationTournamentSettingsId);
    }

    private TournamentSettings GetTournamentSettingsOrThrow(int tournamentSettingsId)
    {
        return tournamentSettingsRepository.GetById(tournamentSettingsId)
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
