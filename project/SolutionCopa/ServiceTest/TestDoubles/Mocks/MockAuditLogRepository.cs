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

    public void Seed(AuditLog log)
    {
        _logs.Add(log);
    }       
}
