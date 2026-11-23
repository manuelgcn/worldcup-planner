using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using WorldCupPlanner.DataAccess;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class DatabaseHealthCheckTests
{
    // Un puerto de esta máquina donde nadie escucha: la conexión falla sin depender de ninguna base.
    private const string UnreachableDatabase = "Server=tcp:127.0.0.1,1;Connect Timeout=1;Encrypt=False";

    // shortcut: la rama sana necesita una base que responda y este proyecto no tiene SQLite ni InMemory;
    // se comprueba a mano con GET /health. Cubrirla acá cuando el proyecto tenga uno de esos proveedores.
    [TestMethod]
    public async Task CheckHealthAsync_WhenTheDatabaseDoesNotAnswer_ReportsTheFailureStatusWithoutDetails()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(UnreachableDatabase)
            .Options;
        Mock<IDbContextFactory<AppDbContext>> factory = new();
        factory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(options));
        HealthCheckContext context = new()
        {
            Registration = new HealthCheckRegistration("database", Mock.Of<IHealthCheck>(), HealthStatus.Degraded, null)
        };

        HealthCheckResult result = await new DatabaseHealthCheck(factory.Object).CheckHealthAsync(context);

        Assert.AreEqual(HealthStatus.Degraded, result.Status);
        Assert.IsNull(result.Description);
        Assert.IsNull(result.Exception);
    }
}
