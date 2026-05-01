using DataAccess.Interfaces;
using ProjectCopa;
using ProjectCopa.Interfaces;
using Services.DTOs;
using Services.Interfaces;

namespace Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStadiumRepository _stadiumRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly IChampConfRepository _championshipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MatchService(
        IMatchRepository matchRepository,
        ITeamRepository teamRepository,
        IStadiumRepository stadiumRepository,
        IGroupRepository groupRepository,
        IJourneyRepository journeyRepository,
        IChampConfRepository championshipRepository,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _stadiumRepository = stadiumRepository;
        _groupRepository = groupRepository;
        _journeyRepository = journeyRepository;
        _championshipRepository = championshipRepository;
        _unitOfWork = unitOfWork;
    }

    public MatchDto CreateMatch(CreateMatchRequestDto request)
    {
        ValidateRequired(request);

        if (_matchRepository.ExistsById(request.MatchId))
        {
            throw new ArgumentException("Match ID already exists");
        }

        var match = BuildMatch(request);
        _matchRepository.Add(match);
        _unitOfWork.Commit();

        return MapToDto(match);
    }

    public MatchDto UpdateMatch(int matchId, UpdateMatchRequestDto request)
    {
        ValidateRequired(request);

        var match = GetMatchOrThrow(matchId);

        if (request.StadiumId.HasValue)
        {
            var stadium = GetStadiumOrThrow(request.StadiumId.Value);
            match.ChangeStadium(stadium);
        }

        _matchRepository.Update(match);
        _unitOfWork.Commit();

        return MapToDto(match);
    }

    public void DeleteMatch(int matchId)
    {
        var match = GetMatchOrThrow(matchId);
        _matchRepository.Delete(match);
        _unitOfWork.Commit();
    }

    public MatchDto GetById(int matchId)
    {
        return MapToDto(GetMatchOrThrow(matchId));
    }

    public List<MatchDto> GetByGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentException("Group name is required");
        }

        return _matchRepository.GetByGroup(groupName).Select(MapToDto).ToList();
    }

    public List<MatchDto> GetByJourney(int journeyNumber)
    {
        return _matchRepository.GetByJourney(journeyNumber).Select(MapToDto).ToList();
    }

    public List<MatchDto> GetByDateRange(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new ArgumentException("'from' must be less than or equal to 'to'");
        }

        return _matchRepository.GetByDateRange(from, to).Select(MapToDto).ToList();
    }

    public void SetGoals(int matchId, int homeGoals, int awayGoals)
    {
        var match = GetMatchOrThrow(matchId);
        match.SetGoals(homeGoals, awayGoals);
        _matchRepository.Update(match);
        _unitOfWork.Commit();
    }

    public void AddGoal(int matchId, bool isHomeTeam)
    {
        var match = GetMatchOrThrow(matchId);

        if (isHomeTeam)
        {
            match.AddHomeGoal();
        }
        else
        {
            match.AddAwayGoal();
        }

        _matchRepository.Update(match);
        _unitOfWork.Commit();
    }

    public void RemoveGoal(int matchId, bool isHomeTeam)
    {
        var match = GetMatchOrThrow(matchId);

        if (isHomeTeam)
        {
            match.RemoveHomeGoal();
        }
        else
        {
            match.RemoveAwayGoal();
        }

        _matchRepository.Update(match);
        _unitOfWork.Commit();
    }

    public void PublishResult(int matchId)
    {
        var match = GetMatchOrThrow(matchId);
        _matchRepository.Update(match);
        _unitOfWork.Commit();
    }

    private Match BuildMatch(CreateMatchRequestDto request)
    {
        var homeTeam = GetTeamOrThrow(request.HomeTeamId);
        var awayTeam = GetTeamOrThrow(request.AwayTeamId);
        var stadium = GetStadiumOrThrow(request.StadiumId);
        var championship = GetChampionshipOrNull(request.ChampionshipConfigurationId);

        if (request.IsElimination)
        {
            var phase = request.Phase ?? throw new ArgumentException("Phase is required");
            var matchCode = request.MatchCode ?? throw new ArgumentException("MatchCode is required");
            var sourceHomeId = request.MatchHomeId ?? throw new ArgumentException("MatchHomeId is required");
            var sourceAwayId = request.MatchAwayId ?? throw new ArgumentException("MatchAwayId is required");
            var sourceHome = GetMatchOrThrow(sourceHomeId);
            var sourceAway = GetMatchOrThrow(sourceAwayId);

            if (championship == null)
            {
                return new MatchElimination(
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

            return new MatchElimination(
                request.MatchId,
                request.MatchDate,
                homeTeam,
                awayTeam,
                stadium,
                championship,
                phase,
                matchCode,
                sourceHome,
                sourceAway);
        }

        var groupId = request.GroupId ?? throw new ArgumentException("GroupId is required");
        var journeyId = request.JourneyId ?? throw new ArgumentException("JourneyId is required");
        var group = GetGroupOrThrow(groupId);
        var journey = GetJourneyOrThrow(journeyId);

        if (championship == null)
        {
            return new MatchGroup(
                request.MatchId,
                request.MatchDate,
                homeTeam,
                awayTeam,
                stadium,
                group,
                journey);
        }

        return new MatchGroup(
            request.MatchId,
            request.MatchDate,
            homeTeam,
            awayTeam,
            stadium,
            championship,
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
        return _matchRepository.GetById(matchId) ?? throw new ArgumentException("Match not found");
    }

    private ITeam GetTeamOrThrow(int teamId)
    {
        return _teamRepository.GetById(teamId) ?? throw new ArgumentException("Team not found");
    }

    private IStadium GetStadiumOrThrow(int stadiumId)
    {
        return _stadiumRepository.GetById(stadiumId) ?? throw new ArgumentException("Stadium not found");
    }

    private IGroup GetGroupOrThrow(int groupId)
    {
        return _groupRepository.GetById(groupId) ?? throw new ArgumentException("Group not found");
    }

    private IJourney GetJourneyOrThrow(int journeyId)
    {
        return _journeyRepository.GetById(journeyId) ?? throw new ArgumentException("Journey not found");
    }

    private IChampConf? GetChampionshipOrNull(int? championshipId)
    {
        if (!championshipId.HasValue)
        {
            return null;
        }

        return _championshipRepository.GetById(championshipId.Value)
               ?? throw new ArgumentException("Championship configuration not found");
    }

    private static void ValidateRequired(object? value)
    {
        if (value == null)
        {
            throw new ArgumentException("Value is required");
        }
    }
}
