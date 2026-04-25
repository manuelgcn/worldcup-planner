using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class MatchGroup : Match
{
    private const string RequiredGroupMessage = "Group is required.";
    private const string RequiredJourneyMessage = "Journey is required.";

    public IGroup Group { get; private set; }
    public IJourney Journey { get; private set; }

    public MatchGroup()
    {
    }

    public MatchGroup(
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

    public MatchGroup(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration,
        IGroup group,
        IJourney journey)
        : base(id, matchDate, homeTeam, awayTeam, stadium, championshipConfiguration)
    {
        EnsureRequiredGroup(group);
        EnsureRequiredJourney(journey);

        Group = group;
        Journey = journey;
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

