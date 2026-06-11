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

    // Filtros combinados aplicados sobre IQueryable: el WHERE se ejecuta en SQL,
    // nunca se materializa la tabla completa para filtrar en memoria.
    // ToLower().Contains se traduce a LOWER(...) LIKE para que accion y actor
    // matcheen parcial y case-insensitive en cualquier collation.
    public List<AuditLog> GetFiltered(DateTime? from, DateTime? to, string? action, string? performedByEmail)
    {
        using AppDbContext context = _contextFactory.CreateDbContext();
        IQueryable<AuditLog> query = context.AuditLogs.AsNoTracking();

        if (from.HasValue)
        {
            query = query.Where(log => log.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(log => log.Timestamp <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            string actionPattern = action.Trim().ToLower();
            query = query.Where(log => log.Action.ToLower().Contains(actionPattern));
        }

        if (!string.IsNullOrWhiteSpace(performedByEmail))
        {
            string actorPattern = performedByEmail.Trim().ToLower();
            query = query.Where(log => log.PerformedByEmail != null && log.PerformedByEmail.ToLower().Contains(actorPattern));
        }

        return query
            .OrderBy(log => log.Timestamp)
            .ThenBy(log => log.Id)
            .ToList();
    }
}
