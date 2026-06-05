using ProjectCopa.Exceptions;

namespace ProjectCopa;

public class MatchElimination : Match
{
    private const string RequiredMatchCodeMessage = "Match code is required.";
    private const string InvalidPhaseMessage = "Phase is invalid.";
    private const string NegativePenaltyGoalsMessage = "Penalty shootout goals cannot be negative.";
    private const string DrawnPenaltyShootoutMessage = "Penalty shootout must produce a winner.";
    private const string PenaltyShootoutOnDecidedMatchMessage = "Penalty shootout can only be recorded when regulation goals are tied.";

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
