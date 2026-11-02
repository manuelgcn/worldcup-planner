using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public class MockAuditLogRepository: IAuditLogRepository
{
    private readonly List<AuditLog> _logs = new List<AuditLog>();

    public void Add(AuditLog log)
    {
        _logs.Add(log);
    }

    public List<AuditLog> GetAll()
    {
        return _logs;
    }

    public List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail, int? take = null)
    {
        IEnumerable<AuditLog> ascending = _logs
            .Where(log => !from.HasValue || log.Timestamp >= from.Value)
            .Where(log => !to.HasValue || log.Timestamp <= to.Value)
            .Where(log => string.IsNullOrWhiteSpace(action)
                || log.Action.Contains(action.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(log => string.IsNullOrWhiteSpace(performedByEmail)
                || (log.PerformedByEmail != null && log.PerformedByEmail.Contains(performedByEmail.Trim(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(log => log.Timestamp)
            .ThenBy(log => log.Id);

        // Reverse del orden estable: ante timestamps iguales, el ultimo insertado sale primero (como el Id en la base).
        return (take.HasValue ? ascending.Reverse().Take(take.Value) : ascending).ToList();
    }

    public void Seed(AuditLog log)
    {
        _logs.Add(log);
    }       
}
