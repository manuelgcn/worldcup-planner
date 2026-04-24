using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Match
{
    private static readonly DateTime DefaultTournamentStartDate = new DateTime(2022, 11, 21);
    private const int InitialGoals = 0;
    private const string InvalidMatchDateMessage = "Match date cannot be earlier than the tournament start date.";
    private const string SameTeamsMessage = "Home team and away team must be different.";
    private const string RequiredStadiumMessage = "Stadium is required.";
    private const string RequiredChampionshipConfigurationMessage = "Championship configuration is required.";

    public int MatchId { get; private set; }
    public DateTime MatchDate { get; private set; }

    private ITeam? _homeTeam;
    private ITeam? _awayTeam;

    public ITeam? HomeTeam
    {
        get => _homeTeam;
        private set
        {
            EnsureDifferentTeams(value, _awayTeam);

            _homeTeam = value;
        }
    }

    public ITeam? AwayTeam
    {
        get => _awayTeam;
        private set
        {
            EnsureDifferentTeams(_homeTeam, value);

            _awayTeam = value;
        }
    }

    public int HomeGoals { get; private set; }
    public int AwayGoals { get; private set; }
    public IStadium? Stadium { get; private set; }

    public Match()
    {
    }

    public Match(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium)
        : this(id, matchDate, homeTeam, awayTeam, stadium, DefaultTournamentStartDate)
    {
    }

    public Match(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration)
        : this(
            id,
            matchDate,
            homeTeam,
            awayTeam,
            stadium,
            GetStartDateOrThrow(championshipConfiguration))
    {
    }

    private Match(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        DateTime minAllowedDate)
    {
        EnsureValidMatchDate(matchDate, minAllowedDate);
        EnsureRequiredStadium(stadium);
        InitializeMatch(id, matchDate, homeTeam, awayTeam, stadium);
    }

    private void InitializeMatch(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium)
    {
        MatchId = id;
        MatchDate = matchDate;
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeGoals = InitialGoals;
        AwayGoals = InitialGoals;
        Stadium = stadium;
    }

    public void ChangeStadium(IStadium stadium)
    {
        EnsureRequiredStadium(stadium);
        Stadium = stadium;
    }

    private static void EnsureValidMatchDate(DateTime matchDate, DateTime minAllowedDate)
    {
        if (matchDate < minAllowedDate)
        {
            throw new MatchException(InvalidMatchDateMessage);
        }
    }

    private static void EnsureDifferentTeams(ITeam? homeTeam, ITeam? awayTeam)
    {
        if (homeTeam != null && awayTeam != null && ReferenceEquals(homeTeam, awayTeam))
        {
            throw new MatchException(SameTeamsMessage);
        }
    }

    private static void EnsureRequiredStadium(IStadium? stadium)
    {
        if (stadium == null)
        {
            throw new MatchException(RequiredStadiumMessage);
        }
    }

    private static DateTime GetStartDateOrThrow(IChampConf? championshipConfiguration)
    {
        if (championshipConfiguration == null)
        {
            throw new MatchException(RequiredChampionshipConfigurationMessage);
        }

        return championshipConfiguration.StartDate();
    }

}