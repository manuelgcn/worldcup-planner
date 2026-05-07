using ProjectCopa.Exceptions;
using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class MatchGroup : Match
{
    private const string RequiredGroupMessage = "Group is required.";
    private const string RequiredJourneyMessage = "Journey is required.";

    public Group? Group { get; private set; }
    public Journey? Journey { get; private set; }

    private MatchGroup(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        Group group,
        Journey journey)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
        EnsureRequiredGroup(group);
        EnsureRequiredJourney(journey);

        Group = group;
        Journey = journey;
    }

    public static MatchGroup CreateInstance(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium, Group group, Journey journey)
    {
        return new MatchGroup(id, matchDate, homeTeam, awayTeam, stadium, group, journey);
    }

    private MatchGroup(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings,
        Group group,
        Journey journey)
        : base(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings)
    {
        EnsureRequiredGroup(group);
        EnsureRequiredJourney(journey);

        Group = group;
        Journey = journey;
    }

    internal static MatchGroup CreateInstance1(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium, TournamentSettings tournamentSettings, Group group, Journey journey)
    {
        return new MatchGroup(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
    }

    public static MatchGroup CreateMatchWithConfig(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium, TournamentSettings tournamentSettings, Group group, Journey journey)
    {
        return CreateInstance1(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
    }

    private static void EnsureRequiredGroup(Group? group)
    {
        if (group == null)
        {
            throw new MatchException(RequiredGroupMessage);
        }
    }

    private static void EnsureRequiredJourney(Journey? journey)
    {
        if (journey == null)
        {
            throw new MatchException(RequiredJourneyMessage);
        }
    }
}
