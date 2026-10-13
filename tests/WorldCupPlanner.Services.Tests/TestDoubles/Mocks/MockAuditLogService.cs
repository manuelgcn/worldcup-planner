using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

// Doble explicito de IAuditLogService para los tests de servicios mutadores, donde la auditoria
// dejo de ser opcional y pasa a ser una dependencia obligatoria. Registra las invocaciones para
// poder verificar que el servicio audita, sin depender de la implementacion real ni de un repo.
// Sigue el patron de los demas mocks de ServiceTest/TestDoubles/Mocks/.
public sealed class MockAuditLogService : IAuditLogService
{
    public List<LoggedAction> LoggedActions { get; } = new();

    public void Log(string action, string details, int tournamentSettingsId, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        LoggedActions.Add(new LoggedAction(action, details, tournamentSettingsId, affectedEntityType, affectedEntityId, null));
    }

    public void Log(string action, string details, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        LoggedActions.Add(new LoggedAction(action, details, null, affectedEntityType, affectedEntityId, null));
    }

    public void LogPerformedBy(string action, string details, string? performedByEmail, string? affectedEntityType = null, int? affectedEntityId = null)
    {
        LoggedActions.Add(new LoggedAction(action, details, null, affectedEntityType, affectedEntityId, performedByEmail));
    }

    public List<AuditLogDto> GetAll()
    {
        return new List<AuditLogDto>();
    }

    public List<AuditLogDto> GetFiltered(AuditLogFilterDto filter)
    {
        return new List<AuditLogDto>();
    }

    public string ExportAsText()
    {
        return string.Empty;
    }

    public sealed record LoggedAction(
        string Action,
        string Details,
        int? TournamentSettingsId,
        string? AffectedEntityType,
        int? AffectedEntityId,
        string? PerformedByEmail);
}
