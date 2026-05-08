using ProjectCopa.Exceptions;

namespace ProjectCopa;

public abstract class Match
{
    private static readonly DateTime DefaultTournamentStartDate = new DateTime(2022, 11, 21);
    private const int InitialGoals = 0;
    private const string InvalidMatchDateMessage = "Match date cannot be earlier than the tournament start date.";
    private const string SameTeamsMessage = "Home team and away team must be different.";
    private const string RequiredStadiumMessage = "Stadium is required.";
    private const string RequiredChampionshipConfigurationMessage = "Championship configuration is required.";
    private const string RequiredHomeSourceMatchMessage = "Home match (source) is required.";
    private const string RequiredAwaySourceMatchMessage = "Away match (source) is required.";
    private const string SameSourceMatchesMessage = "Home and away source matches must be different.";
    private const string GoalsCannotBeNegativeMessage = "Goals cannot be negative.";
    private const string CannotRemoveGoalWhenZeroMessage = "Cannot remove a goal when the current count is zero.";

    public int MatchId { get; private set; }
    public DateTime MatchDate { get; private set; }

    private Team? _homeTeam;
    private Team? _awayTeam;

    public Team? HomeTeam
    {
        get => _homeTeam;
        private set
        {
            EnsureDifferentTeams(value, _awayTeam);

            _homeTeam = value;
        }
    }

    public Team? AwayTeam
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
    public Stadium? Stadium { get; private set; }

    protected Match()
    {
    }

    protected Match(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
        : this(id, matchDate, homeTeam, awayTeam, stadium, DefaultTournamentStartDate)
    {
    }

    protected Match(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings)
        : this(
            id,
            matchDate,
            homeTeam,
            awayTeam,
            stadium,
            GetStartDateOrThrow(tournamentSettings))
    {
    }

    private Match(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        DateTime minAllowedDate)
    {
        EnsureValidMatchDate(matchDate, minAllowedDate);
        EnsureRequiredStadium(stadium);
        InitializeMatch(id, matchDate, homeTeam, awayTeam, stadium);
    }

    private void InitializeMatch(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
    {
        MatchId = id;
        MatchDate = matchDate;
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeGoals = InitialGoals;
        AwayGoals = InitialGoals;
        Stadium = stadium;
    }

    public void ChangeStadium(Stadium stadium)
    {
        EnsureRequiredStadium(stadium);
        Stadium = stadium;
    }

    public void SetGoals(int homeGoals, int awayGoals)
    {
        EnsureNonNegativeGoals(homeGoals, awayGoals);
        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
    }

    public void SetHomeGoals(int goals)
    {
        EnsureNonNegativeGoal(goals);
        HomeGoals = goals;
    }

    public void SetAwayGoals(int goals)
    {
        EnsureNonNegativeGoal(goals);
        AwayGoals = goals;
    }

    public void AddHomeGoal()
    {
        HomeGoals++;
    }

    public void AddAwayGoal()
    {
        AwayGoals++;
    }

    public void RemoveHomeGoal()
    {
        EnsureCanRemoveGoal(HomeGoals);
        HomeGoals--;
    }

    public void RemoveAwayGoal()
    {
        EnsureCanRemoveGoal(AwayGoals);
        AwayGoals--;
    }

    public bool HasHomeTeamWon()
    {
        return HomeGoals > AwayGoals;
    }

    public bool HasAwayTeamWon()
    {
        return AwayGoals > HomeGoals;
    }

    public bool IsDraw()
    {
        return HomeGoals == AwayGoals;
    }

    public int GetGoalDifferenceForHome()
    {
        return HomeGoals - AwayGoals;
    }

    public int GetGoalDifferenceForAway()
    {
        return AwayGoals - HomeGoals;
    }

    public Team? GetWinner()
    {
        if (HasHomeTeamWon())
        {
            return HomeTeam;
        }

        if (HasAwayTeamWon())
        {
            return AwayTeam;
        }

        return null;
    }

    private static void EnsureValidMatchDate(DateTime matchDate, DateTime minAllowedDate)
    {
        if (matchDate < minAllowedDate)
        {
            throw new MatchException(InvalidMatchDateMessage);
        }
    }

    private static void EnsureDifferentTeams(Team? homeTeam, Team? awayTeam)
    {
        if (homeTeam != null && awayTeam != null && ReferenceEquals(homeTeam, awayTeam))
        {
            throw new MatchException(SameTeamsMessage);
        }
    }

    private static void EnsureRequiredStadium(Stadium? stadium)
    {
        if (stadium == null)
        {
            throw new MatchException(RequiredStadiumMessage);
        }
    }

    private static DateTime GetStartDateOrThrow(TournamentSettings? tournamentSettings)
    {
        if (tournamentSettings == null)
        {
            throw new MatchException(RequiredChampionshipConfigurationMessage);
        }

        return tournamentSettings.StartDate;
    }

    protected static void EnsureRequiredHomeSourceMatch(Match? matchHome)
    {
        if (matchHome == null)
        {
            throw new MatchException(RequiredHomeSourceMatchMessage);
        }
    }

    protected static void EnsureRequiredAwaySourceMatch(Match? matchAway)
    {
        if (matchAway == null)
        {
            throw new MatchException(RequiredAwaySourceMatchMessage);
        }
    }

    protected static void EnsureDifferentSourceMatches(Match? matchHome, Match? matchAway)
    {
        if (matchHome != null && matchAway != null && ReferenceEquals(matchHome, matchAway))
        {
            throw new MatchException(SameSourceMatchesMessage);
        }
    }

    private static void EnsureNonNegativeGoals(int homeGoals, int awayGoals)
    {
        EnsureNonNegativeGoal(homeGoals);
        EnsureNonNegativeGoal(awayGoals);
    }

    private static void EnsureNonNegativeGoal(int goals)
    {
        if (goals < 0)
        {
            throw new MatchException(GoalsCannotBeNegativeMessage);
        }
    }

    private static void EnsureCanRemoveGoal(int goals)
    {
        if (goals == 0)
        {
            throw new MatchException(CannotRemoveGoalWhenZeroMessage);
        }
    }

}
