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
    private readonly IUserRepository? _userRepository;

    private AuditLogService(IAuditLogRepository repository, IUserRepository? userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    // Sin userRepository (tests que no miran al actor) los registros quedan sin id de usuario.
    public static AuditLogService CreateInstance(IAuditLogRepository repository, IUserRepository? userRepository = null)
    {
        return new AuditLogService(repository, userRepository);
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

    public void LogPerformedBy(string action, string details, int tournamentSettingsId, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        AddLog(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId, performedByEmail);
    }

    private void AddLog(string action, string details, int? tournamentSettingsId, string? affectedEntityType, int? affectedEntityId, string? performedByEmail)
    {
        AuditLog log = AuditLog.Create(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId, performedByEmail, ResolveUserId(performedByEmail));
        _repository.Add(log);
    }

    // El email del actor se puede cambiar despues: el id se resuelve ahora, mientras todavia es el suyo.
    private int? ResolveUserId(string? performedByEmail)
    {
        if (string.IsNullOrWhiteSpace(performedByEmail))
        {
            return null;
        }

        return _userRepository?.GetByEmail(performedByEmail.Trim())?.idUser;
    }

    public List<AuditLogDto> GetFiltered(AuditLogFilterDto filter, int? take = null)
    {
        ArgumentNullException.ThrowIfNull(filter);

        List<AuditLog> logs = _repository.GetFiltered(filter.From, filter.To, filter.Action, filter.PerformedByEmail, take);
        return logs
            .Select(MapToDto)
            .ToList();
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
            PerformedByUserId = log.PerformedByUserId,
            EntityType = log.AffectedEntityType,
            EntityId = log.AffectedEntityId
        };
    }
}
