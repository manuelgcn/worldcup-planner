using System.Text;
using WorldCupPlanner.Domain;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Los logs de auditoria son globales (del sistema): TournamentSettingsId solo se
// registra cuando el caller lo pasa explicitamente porque aporta contexto real.
public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    private AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public static AuditLogService CreateInstance(IAuditLogRepository repository)
    {
        return new AuditLogService(repository);
    }

    public void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        AddLog(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId, performedByEmail: null);
    }

    public void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        AddLog(action, details, tournamentSettingsId: null, affectedEntityType, affectedEntityId, performedByEmail: null);
    }

    public void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        AddLog(action, details, tournamentSettingsId: null, affectedEntityType, affectedEntityId, performedByEmail);
    }

    private void AddLog(string action, string details, int? tournamentSettingsId, string? affectedEntityType, int? affectedEntityId, string? performedByEmail)
    {
        AuditLog log = AuditLog.Create(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId, performedByEmail);
        _repository.Add(log);
    }

    public List<AuditLogDto> GetAll()
    {
        List<AuditLog> logs = _repository.GetAll();
        return logs
            .Select(MapToDto)
            .ToList();
    }

    public List<AuditLogDto> GetFiltered(AuditLogFilterDto filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        List<AuditLog> logs = _repository.GetFiltered(filter.From, filter.To, filter.Action, filter.PerformedByEmail);
        return logs
            .Select(MapToDto)
            .ToList();
    }

    public string ExportAsText()
    {
        List<AuditLog> logs = _repository.GetAll();

        StringBuilder sb = new StringBuilder();
        foreach (AuditLog log in logs.OrderBy(l => l.Timestamp))
        {
            sb.AppendLine(FormatLogLine(log));
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatLogLine(AuditLog log)
    {
        StringBuilder line = new StringBuilder();
        line.Append($"[{log.Timestamp.ToString("o")}] Action: {log.Action} | Details: {log.Details}");

        if (!string.IsNullOrWhiteSpace(log.PerformedByEmail))
        {
            line.Append($" | PerformedBy: {log.PerformedByEmail}");
        }

        if (log.TournamentSettingsId.HasValue)
        {
            line.Append($" | TournamentID: {log.TournamentSettingsId.Value}");
        }

        if (ExistLogEntity(log))
        {
            line.Append($" | Entity: {log.AffectedEntityType} - ID: {log.AffectedEntityId!.Value}");
        }

        return line.ToString();
    }

    private static bool ExistLogEntity(AuditLog log)
    {
        return !string.IsNullOrWhiteSpace(log.AffectedEntityType) && log.AffectedEntityId.HasValue;
    }

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Timestamp = log.Timestamp,
            Action = log.Action,
            Details = log.Details,
            TournamentSettingsId = log.TournamentSettingsId,
            PerformedByEmail = log.PerformedByEmail,
            EntityType = log.AffectedEntityType,
            EntityId = log.AffectedEntityId
        };
    }
}
