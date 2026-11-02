using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IAuditLogService
{
    void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null);
    void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null);

    void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null);

    List<AuditLogDto> GetAll();
    // Con take devuelve solo los 'take' registros mas nuevos, del mas nuevo al mas viejo.
    List<AuditLogDto> GetFiltered(AuditLogFilterDto filter, int? take = null);
    string ExportAsText();
}
