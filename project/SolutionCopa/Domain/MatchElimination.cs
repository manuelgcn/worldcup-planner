using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class MatchElimination : Match
{
    private const string RequiredMatchCodeMessage = "Match code is required.";
    private const string RequiredMatchHomeMessage = "Home match (source) is required.";
    private const string RequiredMatchAwayMessage = "Away match (source) is required.";
    private const string SameSourceMatchesMessage = "Home and away source matches must be different.";

    public string MatchCode { get; private set; }
    private Match? _matchHome;
    private Match? _matchAway;

    public Match? MatchHome
    {
        get => _matchHome;
        private set
        {
            EnsureRequiredMatchHome(value);
            _matchHome = value;
        }
    }

    public Match? MatchAway
    {
        get => _matchAway;
        private set
        {
            EnsureRequiredMatchAway(value);
            _matchAway = value;
        }
    }

    public MatchElimination()
    {
    }

    public MatchElimination(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        string matchCode,
        Match matchHome,
        Match matchAway)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
        InitializeEliminationData(matchCode, matchHome, matchAway);
    }

    public MatchElimination(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration,
        string matchCode,
        Match matchHome,
        Match matchAway)
        : base(id, matchDate, homeTeam, awayTeam, stadium, championshipConfiguration)
    {
        InitializeEliminationData(matchCode, matchHome, matchAway);
    }

    private void InitializeEliminationData(string matchCode, Match matchHome, Match matchAway)
    {
        EnsureRequiredMatchCode(matchCode);
        EnsureRequiredMatchHome(matchHome);
        EnsureRequiredMatchAway(matchAway);
        EnsureDifferentSourceMatches(matchHome, matchAway);

        MatchCode = matchCode;
        MatchHome = matchHome;
        MatchAway = matchAway;
    }

    private static void EnsureRequiredMatchCode(string? matchCode)
    {
        if (string.IsNullOrWhiteSpace(matchCode))
        {
            throw new MatchException(RequiredMatchCodeMessage);
        }
    }

    private static void EnsureRequiredMatchHome(Match? matchHome)
    {
        if (matchHome == null)
        {
            throw new MatchException(RequiredMatchHomeMessage);
        }
    }

    private static void EnsureRequiredMatchAway(Match? matchAway)
    {
        if (matchAway == null)
        {
            throw new MatchException(RequiredMatchAwayMessage);
        }
    }

    private static void EnsureDifferentSourceMatches(Match matchHome, Match matchAway)
    {
        if (ReferenceEquals(matchHome, matchAway))
        {
            throw new MatchException(SameSourceMatchesMessage);
        }
    }
}
