using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

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

    public List<AuditLog> GetByDateRange(DateTime from, DateTime to)
    {
        return _logs
            .Where(log => log.Timestamp >= from && log.Timestamp <= to)
            .ToList();
    }

    public List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail)
    {
        return _logs
            .Where(log => !from.HasValue || log.Timestamp >= from.Value)
            .Where(log => !to.HasValue || log.Timestamp <= to.Value)
            .Where(log => string.IsNullOrWhiteSpace(action)
                || log.Action.Contains(action.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(log => string.IsNullOrWhiteSpace(performedByEmail)
                || (log.PerformedByEmail != null && log.PerformedByEmail.Contains(performedByEmail.Trim(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(log => log.Timestamp)
            .ThenBy(log => log.Id)
            .ToList();
    }

    public void Seed(AuditLog log)
    {
        _logs.Add(log);
    }       
}
