using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog log);

    List<AuditLog> GetAll();

    List<AuditLog> GetByDateRange(DateTime from, DateTime to);

    List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail);
}
