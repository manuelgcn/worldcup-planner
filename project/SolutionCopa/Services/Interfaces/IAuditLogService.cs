using Services.DTOs;

namespace Services.Interfaces;

public interface IAuditLogService
{
    void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null);
    void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null);

    void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null);

    List<AuditLogDto> GetAll();
    List<AuditLogDto> GetFiltered(AuditLogFilterDto filter);
    string ExportAsText();
}
