using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Match
{
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