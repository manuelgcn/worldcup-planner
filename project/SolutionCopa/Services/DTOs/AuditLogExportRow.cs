namespace Services.DTOs;

public sealed class AuditLogExportRow
{
    public DateTime Timestamp { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public int TournamentSettingsId { get; init; }
    public string? EntityType { get; init; }
    public int? EntityId { get; init; }
}
