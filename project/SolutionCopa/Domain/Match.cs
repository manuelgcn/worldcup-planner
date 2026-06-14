using System.Linq;
using ProjectCopa.Exceptions;

namespace ProjectCopa;

public abstract class Match
{
    private static readonly DateTime DefaultTournamentStartDate = new DateTime(2022, 11, 21);
    private const int InitialGoals = 0;
    private const string InvalidMatchDateMessage = "La fecha del partido no puede ser anterior a la fecha de inicio del torneo.";
    private const string SameTeamsMessage = "El equipo local y el visitante deben ser distintos.";
    private const string RequiredStadiumMessage = "El estadio es obligatorio.";
    private const string RequiredChampionshipConfigurationMessage = "La configuracion del campeonato es obligatoria.";
    private const string RequiredHomeSourceMatchMessage = "El partido de origen local es obligatorio.";
    private const string RequiredAwaySourceMatchMessage = "El partido de origen visitante es obligatorio.";
    private const string SameSourceMatchesMessage = "Los partidos de origen local y visitante deben ser distintos.";
    private const string GoalsCannotBeNegativeMessage = "Los goles no pueden ser negativos.";
    private const string CannotRemoveGoalWhenZeroMessage = "No se puede quitar un gol cuando el marcador esta en cero.";
    private const string IncidentTeamNotInMatchMessage = "El equipo de la incidencia debe ser uno de los dos que disputan el partido.";
    private const string IncidentNotInMatchMessage = "La incidencia no pertenece a este partido.";

    public int MatchId { get; private set; }
    public DateTime MatchDate { get; private set; }

    private Team? _homeTeam;
    private Team? _awayTeam;
    private readonly List<MatchIncident> _incidents = new();

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
    public bool WasSimulated { get; private set; }

    // Estado explicito de "resultado registrado": queda en true cuando se cargan goles (manual)
    // o se simula el partido. Es la unica fuente de verdad de "partido jugado": un 0-0 cargado
    // a mano cuenta como jugado, cosa que la heuristica de goles no distinguia.
    public bool HasResult { get; private set; }

    // Evita aplicar el Elo dos veces sobre el mismo partido (p. ej. simular y luego publicar).
    public bool RankingApplied { get; private set; }

    public IReadOnlyCollection<MatchIncident> Incidents => _incidents.AsReadOnly();

    public void MarkAsSimulated()
    {
        WasSimulated = true;
        HasResult = true;
    }

    public void MarkRankingApplied()
    {
        RankingApplied = true;
    }

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
        HasResult = true;
    }

    public void SetHomeGoals(int goals)
    {
        EnsureNonNegativeGoal(goals);
        HomeGoals = goals;
        HasResult = true;
    }

    public void SetAwayGoals(int goals)
    {
        EnsureNonNegativeGoal(goals);
        AwayGoals = goals;
        HasResult = true;
    }

    public void AddHomeGoal()
    {
        HomeGoals++;
        HasResult = true;
    }

    public void AddAwayGoal()
    {
        AwayGoals++;
        HasResult = true;
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

    public virtual Team? GetWinner()
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

    public void AddIncident(MatchIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);
        EnsureIncidentTeamBelongsToMatch(incident.Team);

        _incidents.Add(incident);
    }

    public void RemoveIncident(MatchIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);
        EnsureIncidentBelongsToMatch(incident);

        _incidents.Remove(incident);
    }

    private void EnsureIncidentBelongsToMatch(MatchIncident incident)
    {
        if (!_incidents.Contains(incident))
        {
            throw new MatchException(IncidentNotInMatchMessage);
        }
    }

    public int CountYellowCards(Team team)
    {
        return CountIncidents<YellowCardIncident>(team);
    }

    public int CountRedCards(Team team)
    {
        return CountIncidents<RedCardIncident>(team);
    }

    private int CountIncidents<TIncident>(Team team) where TIncident : MatchIncident
    {
        return _incidents.Count(incident => incident is TIncident && ReferenceEquals(incident.Team, team));
    }

    private void EnsureIncidentTeamBelongsToMatch(Team team)
    {
        if (!ReferenceEquals(team, _homeTeam) && !ReferenceEquals(team, _awayTeam))
        {
            throw new MatchException(IncidentTeamNotInMatchMessage);
        }
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

    public void ChangeDate(DateTime newDate)
    {
        EnsureValidMatchDate(newDate, DefaultTournamentStartDate);
        MatchDate = newDate;
    }
}
