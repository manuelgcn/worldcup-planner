using Microsoft.Extensions.DependencyInjection;
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
}
