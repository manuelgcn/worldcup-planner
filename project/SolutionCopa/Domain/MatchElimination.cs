using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class MatchElimination : Match
{
    private const string RequiredMatchCodeMessage = "Match code is required.";
    private const string InvalidPhaseMessage = "Phase is invalid.";

    public string MatchCode { get; private set; } = null!;
    public Phase Phase { get; private set; }
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

    private MatchElimination(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
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
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration,
        Phase phase,
        string matchCode,
        Match matchHome,
        Match matchAway)
        : base(id, matchDate, homeTeam, awayTeam, stadium, championshipConfiguration)
    {
        InitializeEliminationData(phase, matchCode, matchHome, matchAway);
    }

    public static MatchElimination Create(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
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
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration,
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
            championshipConfiguration,
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

}
