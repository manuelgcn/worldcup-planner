using WorldCupPlanner.Domain.Exceptions;

namespace WorldCupPlanner.Domain;

public class MatchGroup : Match
{
    private const string RequiredGroupMessage = "El grupo es obligatorio.";
    private const string RequiredJourneyMessage = "La jornada es obligatoria.";
    private const string MatchCodeRequiredMessage = "El codigo de partido es obligatorio.";

    public Group? Group { get; private set; }
    public Journey? Journey { get; private set; }
    public string MatchCode { get; private set; } = null!;

    public void SetMatchCode(string matchCode)
    {
        if (string.IsNullOrWhiteSpace(matchCode))
        {
            throw new MatchException(MatchCodeRequiredMessage);
        }

        MatchCode = matchCode;
    }

    // Constructor sin parametros exclusivo para la materializacion de EF Core (TPH).
    // EF no puede enlazar las navegaciones (Group/Journey) por constructor, asi que usa este
    // y escribe las propiedades/backing fields directamente. No usar desde la app: las factories
    // estaticas siguen siendo la unica via de creacion con invariantes.
    private MatchGroup()
    {
    }

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

    private static MatchGroup CreateInstance(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium, TournamentSettings tournamentSettings, Group group, Journey journey)
    {
        return new MatchGroup(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
    }

    public static MatchGroup CreateMatchWithConfig(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium, TournamentSettings tournamentSettings, Group group, Journey journey)
    {
        return CreateInstance(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings, group, journey);
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
