using WorldCupPlanner.Domain.Exceptions;

namespace WorldCupPlanner.Domain;

// OCP: nuevos tipos de incidencia (sustitucion, VAR, ...) se agregan heredando
// de esta clase base, sin tocar Match ni los tipos existentes.
public abstract class MatchIncident
{
    private const string RequiredTeamMessage = "El equipo de la incidencia es obligatorio.";
    private const string NegativeMinuteMessage = "El minuto de la incidencia no puede ser negativo.";

    public int Id { get; private set; }
    public int Minute { get; private set; }
    public Team Team { get; private set; } = null!;

    protected MatchIncident()
    {
    }

    protected MatchIncident(int id, int minute, Team team)
    {
        EnsureRequiredTeam(team);
        EnsureNonNegativeMinute(minute);

        Id = id;
        Minute = minute;
        Team = team;
    }

    private static void EnsureRequiredTeam(Team team)
    {
        if (team is null)
        {
            throw new MatchException(RequiredTeamMessage);
        }
    }

    private static void EnsureNonNegativeMinute(int minute)
    {
        if (minute < 0)
        {
            throw new MatchException(NegativeMinuteMessage);
        }
    }
}
