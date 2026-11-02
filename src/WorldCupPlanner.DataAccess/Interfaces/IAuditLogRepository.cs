using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog log);

    List<AuditLog> GetAll();

    // Sin take: todas las filas, de la mas vieja a la mas nueva. Con take: las 'take' mas nuevas primero.
    List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail, int? take = null);
}
