using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WorldCupPlanner.DataAccess;

namespace WorldCupPlanner.UI;

// Respalda GET /health: la app está sana solo si la base responde. No devuelve descripción ni
// excepción porque el endpoint es anónimo y el texto del cliente SQL revelaría servidor y usuario.
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public DatabaseHealthCheck(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using AppDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(context.Registration.FailureStatus);
    }
}
