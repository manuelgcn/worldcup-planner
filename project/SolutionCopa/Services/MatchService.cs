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
    IUnitOfWork unitOfWork)
    : IMatchService
{
    public MatchDto CreateMatch(CreateMatchRequestDto request)
    {
        ValidateRequired(request);

        if (matchRepository.ExistsById(request.MatchId))
        {
            throw new MatchServicesException("Match ID already exists");
        }

        Match match = BuildMatch(request);
        matchRepository.Add(match);
        unitOfWork.Commit();

        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        Match match = GetMatchOrThrow(matchId);

        if (request.StadiumId.HasValue)
        {
            var stadium = GetStadiumOrThrow(request.StadiumId.Value);
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
            throw new MatchServicesException("Group name is required");
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
            throw new MatchServicesException("'from' must be less than or equal to 'to'");
        }

        return matchRepository.GetByDateRange(from, to).Select(MapToDto).ToList();
    }

    public void SetGoals(int matchId, int homeGoals, int awayGoals)
    {
        var match = GetMatchOrThrow(matchId);
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
        var match = GetMatchOrThrow(matchId);
        matchRepository.Update(match);
        unitOfWork.Commit();
    }

    private Match BuildMatch(CreateMatchRequestDto request)
    {
        Team homeTeam = GetTeamOrThrow(request.HomeTeamId);
        Team awayTeam = GetTeamOrThrow(request.AwayTeamId);
        Stadium stadium = GetStadiumOrThrow(request.StadiumId);
        TournamentSettings? tournamentSettings = GetTournamentSettingsOrNull(request.ChampionshipConfigurationId);

        if (request.IsElimination)
        {
            Phase phase = request.Phase ?? throw new MatchServicesException("Phase is required");
            string matchCode = request.MatchCode ?? throw new MatchServicesException("MatchCode is required");
            int sourceHomeId = request.MatchHomeId ?? throw new MatchServicesException("MatchHomeId is required");
            int sourceAwayId = request.MatchAwayId ?? throw new MatchServicesException("MatchAwayId is required");
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

        int groupId = request.GroupId ?? throw new MatchServicesException("GroupId is required");
        int journeyId = request.JourneyId ?? throw new MatchServicesException("JourneyId is required");
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
        return matchRepository.GetById(matchId) ?? throw new MatchServicesException("Match not found");
    }

    private Team GetTeamOrThrow(int teamId)
    {
        return teamRepository.GetById(teamId) ?? throw new MatchServicesException("Team not found");
    }

    private Stadium GetStadiumOrThrow(int stadiumId)
    {
        return stadiumRepository.GetById(stadiumId) ?? throw new MatchServicesException("Stadium not found");
    }

    private Group GetGroupOrThrow(int groupId)
    {
        return groupRepository.GetById(groupId) ?? throw new MatchServicesException("Group not found");
    }

    private Journey GetJourneyOrThrow(int journeyId)
    {
        return journeyRepository.GetById(journeyId) ?? throw new MatchServicesException("Journey not found");
    }

    private TournamentSettings? GetTournamentSettingsOrNull(int? tournamentSettingsId)
    {
        if (!tournamentSettingsId.HasValue)
        {
            return null;
        }

        return tournamentSettingsRepository.GetById(tournamentSettingsId.Value)
               ?? throw new MatchServicesException("Tournament settings not found");
    }

    private static void ValidateRequired(object? value)
    {
        if (value == null)
        {
            throw new MatchServicesException("Value is required");
        }
    }
}
