using WorldCupPlanner.Domain.Exceptions;

namespace WorldCupPlanner.Domain;

public class MatchElimination : Match
{
    private const string RequiredMatchCodeMessage = "El codigo de partido es obligatorio.";
    private const string InvalidPhaseMessage = "La fase es invalida.";
    private const string NegativePenaltyGoalsMessage = "Los goles de la definicion por penales no pueden ser negativos.";
    private const string DrawnPenaltyShootoutMessage = "La definicion por penales debe tener un ganador.";
    private const string PenaltyShootoutOnDecidedMatchMessage = "La definicion por penales solo puede registrarse cuando los goles del tiempo reglamentario estan empatados.";

    public string MatchCode { get; private set; } = null!;
    public Phase Phase { get; private set; }
    public bool WonByPenalties { get; private set; }
    public int HomePenaltyGoals { get; private set; }
    public int AwayPenaltyGoals { get; private set; }
    private Match? _matchHome;
    private Match? _matchAway;

    public Match? MatchHome
    {
        get => _matchHome;
        private set
        {
            EnsureRequiredHomeSourceMatch(value);
            _matchHome = value;
        }
    }

    public Match? MatchAway
    {
        get => _matchAway;
        private set
        {
            EnsureRequiredAwaySourceMatch(value);
            _matchAway = value;
        }
    }

    // Constructor sin parametros exclusivo para la materializacion de EF Core (TPH).
    // EF escribe MatchHome/MatchAway por backing field (_matchHome/_matchAway) evitando los
    // setters con validacion. No usar desde la app: las factories estaticas conservan las invariantes.
    private MatchElimination()
    {
    }

    private MatchElimination(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        Phase phase,
        string matchCode,
        Match matchHome,
        Match matchAway)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
        InitializeEliminationData(phase, matchCode, matchHome, matchAway);
    }

    private MatchElimination(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings,
        Phase phase,
        string matchCode,
        Match matchHome,
        Match matchAway)
        : base(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings)
    {
        InitializeEliminationData(phase, matchCode, matchHome, matchAway);
    }

    // Constructor de la ronda inicial (R32): sin partidos fuente.
    private MatchElimination(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings,
        Phase phase,
        string matchCode)
        : base(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings)
    {
        InitializeInitialRoundData(phase, matchCode);
    }

    public static MatchElimination Create(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        Phase phase,
        string matchCode,
        Match matchHome,
        Match matchAway)
    {
        return new MatchElimination(id, matchDate, homeTeam, awayTeam, stadium, phase, matchCode, matchHome, matchAway);
    }

    public static MatchElimination CreateMatchWithConfig(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings,
        Phase phase,
        string matchCode,
        Match matchHome,
        Match matchAway)
    {
        return new MatchElimination(
            id,
            matchDate,
            homeTeam,
            awayTeam,
            stadium,
            tournamentSettings,
            phase,
            matchCode,
            matchHome,
            matchAway);
    }

    // Factory para la ronda INICIAL del bracket (R32). Sus emparejamientos salen directamente de la
    // clasificacion de grupos, no de partidos previos, asi que NO recibe ni setea partidos fuente:
    // MatchHome/MatchAway quedan null (sus tipos ya son nullables). Las rondas posteriores (R16 en
    // adelante) siguen creandose con Create/CreateMatchWithConfig, que exigen fuentes reales ya
    // persistidas. Mantener separadas ambas vias evita fabricar fuentes sinteticas para R32.
    public static MatchElimination CreateInitialRound(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings,
        Phase phase,
        string matchCode)
    {
        return new MatchElimination(
            id,
            matchDate,
            homeTeam,
            awayTeam,
            stadium,
            tournamentSettings,
            phase,
            matchCode);
    }

    private void InitializeEliminationData(Phase phase, string matchCode, Match matchHome, Match matchAway)
    {
        EnsureValidPhase(phase);
        EnsureRequiredMatchCode(matchCode);
        EnsureRequiredHomeSourceMatch(matchHome);
        EnsureRequiredAwaySourceMatch(matchAway);
        EnsureDifferentSourceMatches(matchHome, matchAway);

        Phase = phase;
        MatchCode = matchCode;
        MatchHome = matchHome;
        MatchAway = matchAway;
    }

    private void InitializeInitialRoundData(Phase phase, string matchCode)
    {
        EnsureValidPhase(phase);
        EnsureRequiredMatchCode(matchCode);

        Phase = phase;
        MatchCode = matchCode;
        // MatchHome / MatchAway quedan null: la ronda inicial no tiene partidos fuente.
    }

    private static void EnsureValidPhase(Phase phase)
    {
        if (!Enum.IsDefined(typeof(Phase), phase))
        {
            throw new MatchException(InvalidPhaseMessage);
        }
    }

    private static void EnsureRequiredMatchCode(string? matchCode)
    {
        if (string.IsNullOrWhiteSpace(matchCode))
        {
            throw new MatchException(RequiredMatchCodeMessage);
        }
    }

    public void SetPenaltyShootoutResult(int homePenaltyGoals, int awayPenaltyGoals)
    {
        if (homePenaltyGoals < 0 || awayPenaltyGoals < 0)
        {
            throw new MatchException(NegativePenaltyGoalsMessage);
        }

        if (homePenaltyGoals == awayPenaltyGoals)
        {
            throw new MatchException(DrawnPenaltyShootoutMessage);
        }

        if (!IsDraw())
        {
            throw new MatchException(PenaltyShootoutOnDecidedMatchMessage);
        }

        HomePenaltyGoals = homePenaltyGoals;
        AwayPenaltyGoals = awayPenaltyGoals;
        WonByPenalties = true;
    }

    public override Team? GetWinner()
    {
        if (HasHomeTeamWon())
        {
            return HomeTeam;
        }

        if (HasAwayTeamWon())
        {
            return AwayTeam;
        }

        if (WonByPenalties)
        {
            return HomePenaltyGoals > AwayPenaltyGoals ? HomeTeam : AwayTeam;
        }

        return null;
    }
}
