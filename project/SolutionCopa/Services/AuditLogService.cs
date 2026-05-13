using System.Text;
using ProjectCopa;
using DataAccess.Interfaces;
using Services.DTOs;
using Services.Interfaces;

namespace Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly int _defaultTournamentSettingsId;
    private readonly IAuditLogRepository _repository;

    private AuditLogService(IAuditLogRepository repository, int defaultTournamentSettingsId)
    {
        _repository = repository;
        _defaultTournamentSettingsId = defaultTournamentSettingsId;
    }

    public static AuditLogService CreateInstance(IAuditLogRepository repository, int defaultTournamentSettingsId)
    {
        return new AuditLogService(repository, defaultTournamentSettingsId);
    }

    public void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        AuditLog log = AuditLog.Create(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId);
        _repository.Add(log);
    }

    public void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        Log(action, details, _defaultTournamentSettingsId, affectedEntityType, affectedEntityId);
    }

    public void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        string detailsWithActor = string.IsNullOrWhiteSpace(performedByEmail)
            ? details
            : $"{details} by {performedByEmail}";

        Log(action, detailsWithActor, _defaultTournamentSettingsId, affectedEntityType, affectedEntityId);
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
        List<AuditLogDto> filtered = new List<AuditLogDto>();
        List<AuditLog> logs = _repository.GetAll();

        foreach (AuditLog log in logs)
        {
            if (log.TournamentSettingsId == tournamentSettingsId)
            {
                filtered.Add(MapToDto(log));
            }
        }

        return filtered;
    }

    public string ExportAsText(int? tournamentSettingsId = null)
    {
        IEnumerable<AuditLog> toExport = GetLogsToExport(tournamentSettingsId);

        StringBuilder sb = new StringBuilder();
        foreach (AuditLog log in toExport.OrderBy(l => l.Timestamp))
        {
            sb.AppendLine(FormatLogLine(log));
        }

        return sb.ToString().TrimEnd(); 
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
            
        if (ExistLogEntity(log))
        {
            entityPart = $" | Entity: {log.AffectedEntityType} - ID: {log.AffectedEntityId!.Value}";
        }

        return $"[{log.Timestamp.ToString("o")}] Action: {log.Action} | Details: {log.Details} | TournamentID: {log.TournamentSettingsId}{entityPart}";
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
            EntityType = log.AffectedEntityType,
            EntityId = log.AffectedEntityId
        };
    }
}
