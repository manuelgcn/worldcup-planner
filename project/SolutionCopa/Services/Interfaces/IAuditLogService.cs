using Services.DTOs;

namespace Services.Interfaces
{
    public interface IAuditLogService
    {
        void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null);

        List<AuditLogDto> GetAll();
        List<AuditLogDto> GetByTournamentSettings(int tournamentSettingsId);
        string ExportAsText(int? tournamentSettingsId = null);
    }
}
