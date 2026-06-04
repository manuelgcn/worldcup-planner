using ProjectCopa.Exceptions;

namespace ProjectCopa;
public sealed class AuditLog
{
    public int  Id { get; private set; }
    public DateTime Timestamp { get; }
    public string Action { get; }
    public string Details { get; }
    public int TournamentSettingsId { get; }
    public string? AffectedEntityType { get; }
    public int? AffectedEntityId { get; }

    // Constructor sin parametros requerido por EF Core para materializar la entidad:
    // el ctor parametrizado usa 'timestampUtc', que no matchea la propiedad 'Timestamp',
    // por lo que EF no puede bindearlo. EF instancia por aca y escribe el resto de las
    // propiedades (incluido el Id IDENTITY) por sus backing fields.
    private AuditLog()
    {
        Action = string.Empty;
        Details = string.Empty;
    }

    private AuditLog(
        DateTime timestampUtc,
        string action,
        string details,
        int tournamentSettingsId,
        string? affectedEntityType = null,
        int? affectedEntityId = null)
    {
        Timestamp = timestampUtc;
        Action = action;
        Details = details;
        TournamentSettingsId = tournamentSettingsId;
        AffectedEntityType = affectedEntityType;
        AffectedEntityId = affectedEntityId;
    }

    private static void EnsureActionIsValid(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new AuditLogException("Action is required for an AuditLog.");
        }
    }

    private static string TrimAction(string action)
    {
        return action.Trim();
    }

    public static AuditLog Create(
        string? action,
        string? details,
        int tournamentSettingsId,
        string? affectedEntityType = null,
        int? affectedEntityId = null)
    {
        EnsureActionIsValid(action);

        string cleanAction = TrimAction(action!);
        string cleanDetails = details ?? string.Empty;
        DateTime timestampUtc = DateTime.UtcNow;

        return new AuditLog(timestampUtc, cleanAction, cleanDetails, tournamentSettingsId, affectedEntityType, affectedEntityId);
    }

    public void SetId(int id)
    {
        Id = id;
    }
}
