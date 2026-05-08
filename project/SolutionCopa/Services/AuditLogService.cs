using System.Text;
using ProjectCopa;
using DataAccess.Interfaces;
using Services.DTOs;
using Services.Interfaces;

namespace Services
{
    public sealed class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public void Log(string action, string details, int tournamentSettingsId, string? entityType = null, int? entityId = null)
        {
            AuditLog log = AuditLog.Create(action, details, tournamentSettingsId, entityType, entityId);
            _repository.Add(log);
        }

        public List<AuditLogDto> GetAll()
        {
            List<AuditLog> logs = _repository.GetAll();
            return logs
                .Select(MapToDto)
                .ToList();
        }

        public List<AuditLogDto> GetByTournamentSettings(int tournamentSettingsId)
        {
            List<AuditLog> logs = _repository.GetAll();
            List<AuditLog> filtered = logs
                .Where(l => l.TournamentSettingsId == tournamentSettingsId)
                .ToList();
            return filtered
                .Select(MapToDto)
                .ToList();
        }

        public string ExportAsText(int? tournamentSettingsId = null)
        {
            IEnumerable<AuditLog> toExport = GetLogsToExport(tournamentSettingsId);

            StringBuilder sb = new StringBuilder();
            foreach (AuditLog log in toExport
                         .OrderBy(l => l.Timestamp))
            {
                sb.AppendLine(FormatLogLine(log));
            }

            return sb.ToString();
        }

        private IEnumerable<AuditLog> GetLogsToExport(int? tournamentSettingsId)
        {
            List<AuditLog> logs = _repository.GetAll();
            if (tournamentSettingsId.HasValue)
            {
                return logs
                    .Where(l => l.TournamentSettingsId == tournamentSettingsId.Value);
            }

            return logs;
        }

        private static string FormatLogLine(AuditLog log)
        {
            string entityPart = string.Empty;
            if (!string.IsNullOrWhiteSpace(log.AffectedEntityType) && log.AffectedEntityId.HasValue)
            {
                entityPart = $" | {log.AffectedEntityType}Id={log.AffectedEntityId.Value}";
            }

            return $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] ACTION={log.Action} | Tournament={log.TournamentSettingsId}{entityPart} | {log.Details}";
        }

        private static AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Timestamp = log.Timestamp,
                Action = log.Action,
                Details = log.Details,
                TournamentSettingsId = log.TournamentSettingsId,
                EntityType = log.AffectedEntityType,
                EntityId = log.AffectedEntityId
            };
        }
    }
}
