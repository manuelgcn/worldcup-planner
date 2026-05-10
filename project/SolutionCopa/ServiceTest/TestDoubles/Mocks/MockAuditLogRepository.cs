using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public class MockAuditLogRepository: IAuditLogRepository
{
    private readonly List<AuditLog> _logs = new();

    public void Add(AuditLog log)
    {
        _logs.Add(log);
    }

    public List<AuditLog> GetAll()
    {
        return _logs;
    }

    public void Seed(AuditLog log)
    {
        _logs.Add(log);
    }       
}