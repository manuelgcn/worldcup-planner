using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess.Repositories;

// Repositorio de AuditLog sobre EF Core (mismo patron que TeamRepository).
// El Id es IDENTITY: lo asigna la BD al guardar; el repo nunca llama a SetId.
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AuditLogRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Add(AuditLog log)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        context.AuditLogs.Add(log);
        context.SaveChanges();                                          // EF asigna el Id (IDENTITY) y lo refleja en la entidad
    }

    public List<AuditLog> GetAll()
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.AuditLogs.AsNoTracking().OrderBy(l => l.Id).ToList();  // orden de insercion (Id ascendente)
    }

    public List<AuditLog> GetByDateRange(DateTime from, DateTime to)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        return context.AuditLogs.AsNoTracking()
            .Where(log => log.Timestamp >= from && log.Timestamp <= to)
            .OrderBy(log => log.Timestamp)
            .ThenBy(log => log.Id)
            .ToList();
    }
}
