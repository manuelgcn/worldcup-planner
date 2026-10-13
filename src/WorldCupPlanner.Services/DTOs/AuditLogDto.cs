namespace WorldCupPlanner.Services.DTOs;
public class AuditLogDto
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int? TournamentSettingsId { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}
