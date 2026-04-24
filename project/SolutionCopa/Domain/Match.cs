using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Match
{
    private static readonly DateTime DefaultTournamentStartDate = new DateTime(2022, 11, 21);

    public int MatchId { get; private set; }
    public DateTime MatchDate { get; private set; }

    private ITeam? _homeTeam;
    private ITeam? _awayTeam;

    public ITeam? HomeTeam
    {
        get => _homeTeam;
        private set
        {
            ValidateDifferentTeams(value, _awayTeam);

            _homeTeam = value;
        }
    }

    public ITeam? AwayTeam
    {
        get => _awayTeam;
        private set
        {
            ValidateDifferentTeams(_homeTeam, value);

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
    {
        if (matchDate < DefaultTournamentStartDate)
            throw new MatchException();

        MatchId = id;
        MatchDate = matchDate;
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeGoals = 0;
        AwayGoals = 0;
        Stadium = stadium;
    }

    public Match(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration)
    {
        if (matchDate < championshipConfiguration.StartDate())
            throw new MatchException();

        MatchId = id;
        MatchDate = matchDate;
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeGoals = 0;
        AwayGoals = 0;
        Stadium = stadium;
    }

    private static void ValidateDifferentTeams(ITeam? homeTeam, ITeam? awayTeam)
    {
        if (homeTeam != null && awayTeam != null && ReferenceEquals(homeTeam, awayTeam))
            throw new MatchException();
    }
}