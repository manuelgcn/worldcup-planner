using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class MatchGroup : Match
{
    private const string RequiredGroupMessage = "Group is required.";
    private const string RequiredJourneyMessage = "Journey is required.";

    public IGroup? Group { get; private set; }
    public IJourney? Journey { get; private set; }

    private MatchGroup(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IGroup group,
        IJourney journey)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
        EnsureRequiredGroup(group);
        EnsureRequiredJourney(journey);

        Group = group;
        Journey = journey;
    }

    public static MatchGroup CreateInstance(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium, IGroup group, IJourney journey)
    {
        return new MatchGroup(id, matchDate, homeTeam, awayTeam, stadium, group, journey);
    }

    private MatchGroup(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        ITournamentSettings tournamentSettings,
        IGroup group,
        IJourney journey)
        : base(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings)
    {
        EnsureRequiredGroup(group);
        EnsureRequiredJourney(journey);

        Group = group;
        Journey = journey;
    }

    internal static MatchGroup CreateInstance1(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium, ITournamentSettings tournamentSettings, IGroup group, IJourney journey)
    {
        return new MatchGroup(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
    }

    public static MatchGroup CreateMatchWithConfig(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium, ITournamentSettings tournamentSettings, IGroup group, IJourney journey)
    {
        return CreateInstance1(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
    }

    private static void EnsureRequiredGroup(IGroup? group)
    {
        if (group == null)
        {
            throw new MatchException(RequiredGroupMessage);
        }
    }

    private static void EnsureRequiredJourney(IJourney? journey)
    {
        if (journey == null)
        {
            throw new MatchException(RequiredJourneyMessage);
        }
    }
}
