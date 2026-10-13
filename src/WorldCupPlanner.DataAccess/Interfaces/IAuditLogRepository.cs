using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog log);

    List<AuditLog> GetAll();

    List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail);
}
