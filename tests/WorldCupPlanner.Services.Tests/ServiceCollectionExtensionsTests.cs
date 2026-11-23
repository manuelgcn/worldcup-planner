using Microsoft.Extensions.DependencyInjection;
using Moq;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddWorldCupPlanner_RegistersAGraphWhereEveryServiceResolves()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddWorldCupPlanner("Server=unused;Database=unused;", "flags");

        // ValidateOnBuild falla si a algun servicio le falta una dependencia registrada.
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.IsNotNull(provider.GetRequiredService<IFixtureOrchestrator>());
        Assert.IsNotNull(provider.GetRequiredService<IKnockoutFixtureGenerator>());
        Assert.AreEqual(2, provider.GetServices<IMatchSimulationEngine>().Count());
    }

    // El repositorio de usuarios es opcional en AuditLogService.CreateInstance: sin este cableado nada falla, solo se pierde el id.
    [TestMethod]
    public void AddWorldCupPlanner_GivesTheAuditLogServiceTheUsersItResolvesActorIdsFrom()
    {
        User actor = new User(7, "Ana", "Admin", "admin@copa.com", "Password@1", new DateTime(1990, 1, 1));
        Mock<IUserRepository> users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetByEmail(actor.email)).Returns(actor);
        Mock<IAuditLogRepository> auditLogs = new Mock<IAuditLogRepository>();
        ServiceCollection services = new ServiceCollection();
        services.AddWorldCupPlanner("Server=unused;Database=unused;", "flags");
        services.AddSingleton(users.Object);        // el ultimo registro gana: reemplazan a los repositorios sobre SQL Server
        services.AddSingleton(auditLogs.Object);
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IAuditLogService>().LogPerformedBy("ACTION", "Details", actor.email);

        auditLogs.Verify(repository => repository.Add(It.Is<AuditLog>(log => log.PerformedByUserId == actor.idUser)), Times.Once);
    }
}
