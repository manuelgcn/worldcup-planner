using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public class MatchFactory(
    ITeamRepository teamRepository,
    IStadiumRepository stadiumRepository,
    IGroupRepository groupRepository,
    IJourneyRepository journeyRepository,
    ITournamentSettingsRepository tournamentSettingsRepository,
    IMatchRepository matchRepository) 
    : IMatchFactory
{
    private const string PhaseRequiredMessage = "Phase is required";
    private const string MatchCodeRequiredMessage = "MatchCode is required";
    private const string MatchHomeIdRequiredMessage = "MatchHomeId is required";
    private const string MatchAwayIdRequiredMessage = "MatchAwayId is required";
    private const string GroupNameRequiredMessage = "Group name is required";
    private const string JourneyIdRequiredMessage = "JourneyId is required";
    private const string TeamNotFoundMessage = "Team not found";
    private const string StadiumNotFoundMessage = "Stadium not found";
    private const string GroupNotFoundMessage = "Group not found";
    private const string JourneyNotFoundMessage = "Journey not found";
    private const string TournamentSettingsNotFoundMessage = "Tournament settings not found";
    private const string MatchNotFoundMessage = "Match not found";

    public Match Create(CreateMatchRequestDto request)
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
                    request.MatchId, request.MatchDate, homeTeam, awayTeam, 
                    stadium, phase, matchCode, sourceHome, sourceAway);
            }

            return MatchElimination.CreateMatchWithConfig(
                request.MatchId, request.MatchDate, homeTeam, awayTeam, 
                stadium, tournamentSettings, phase, matchCode, sourceHome, sourceAway);
        }

        string groupName = request.GroupName ?? throw new MatchServicesException(GroupNameRequiredMessage);
        int journeyId = request.JourneyId ?? throw new MatchServicesException(JourneyIdRequiredMessage);
        
        Group group = GetGroupOrThrow(groupName);
        Journey journey = GetJourneyOrThrow(journeyId);

        if (tournamentSettings == null)
        {
            return MatchGroup.CreateInstance(
                request.MatchId, request.MatchDate, homeTeam, awayTeam, 
                stadium, group, journey);
        }

        return MatchGroup.CreateMatchWithConfig(
            request.MatchId, request.MatchDate, homeTeam, awayTeam, 
            stadium, tournamentSettings, group, journey);
    }
    
    public void ApplyUpdate(Match match, UpdateMatchRequestDto request)
    {
        if (request.StadiumId.HasValue)
        {
            Stadium stadium = GetStadiumOrThrow(request.StadiumId.Value);
            match.ChangeStadium(stadium);
        }
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

    private Group GetGroupOrThrow(string groupName)
    {
        return groupRepository.GetByName(groupName) ?? throw new MatchServicesException(GroupNotFoundMessage);
    }

    private Journey GetJourneyOrThrow(int journeyId)
    {
        return journeyRepository.GetByNumber(journeyId) ?? throw new MatchServicesException(JourneyNotFoundMessage);
    }

    private TournamentSettings? GetTournamentSettingsOrNull(int? tournamentSettingsId)
    {
        if (!tournamentSettingsId.HasValue) return null;
        
        return tournamentSettingsRepository.GetById(tournamentSettingsId.Value)
               ?? throw new MatchServicesException(TournamentSettingsNotFoundMessage);
    }
}
