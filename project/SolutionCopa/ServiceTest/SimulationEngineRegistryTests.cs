using Services;
using Services.Exceptions;
using Services.Interfaces;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class SimulationEngineRegistryTests
{
    private const string ProbabilisticName = "Probabilístico";
    private const string PureRandomName = "Aleatorio Puro";

    [TestMethod]
    public void AvailableEngineNames_ListsEnginesInRegistrationOrder()
    {
        SimulationEngineRegistry registry = CreateRegistryWithBothEngines();

        CollectionAssert.AreEqual(
            new[] { ProbabilisticName, PureRandomName },
            registry.AvailableEngineNames.ToList());
    }

    [TestMethod]
    public void Resolve_WithKnownName_ReturnsEngineWithThatName()
    {
        SimulationEngineRegistry registry = CreateRegistryWithBothEngines();

        IMatchSimulationEngine engine = registry.Resolve(PureRandomName);

        Assert.AreEqual(PureRandomName, engine.Name);
        Assert.IsInstanceOfType(engine, typeof(PureRandomSimulationEngine));
    }

    [TestMethod]
    public void Resolve_WithUnknownName_ThrowsSimulationEngineException()
    {
        SimulationEngineRegistry registry = CreateRegistryWithBothEngines();

        Assert.ThrowsException<SimulationEngineException>(() => registry.Resolve("Inexistente"));
    }

    [TestMethod]
    public void Resolve_WithNull_ThrowsSimulationEngineException()
    {
        SimulationEngineRegistry registry = CreateRegistryWithBothEngines();

        Assert.ThrowsException<SimulationEngineException>(() => registry.Resolve(null!));
    }

    [TestMethod]
    public void Resolve_WithWhitespace_ThrowsSimulationEngineException()
    {
        SimulationEngineRegistry registry = CreateRegistryWithBothEngines();

        Assert.ThrowsException<SimulationEngineException>(() => registry.Resolve("   "));
    }

    [TestMethod]
    public void Constructor_WithDuplicateEngineNames_ThrowsSimulationEngineException()
    {
        MockMatchSimulationEngine firstEngine = new MockMatchSimulationEngine { Name = ProbabilisticName };
        MockMatchSimulationEngine duplicatedEngine = new MockMatchSimulationEngine { Name = ProbabilisticName };

        Assert.ThrowsException<SimulationEngineException>(
            () => new SimulationEngineRegistry(new IMatchSimulationEngine[] { firstEngine, duplicatedEngine }));
    }

    [TestMethod]
    public void Constructor_WithNullEngines_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new SimulationEngineRegistry(null!));
    }

    [TestMethod]
    public void Constructor_WithoutEngines_ThrowsSimulationEngineException()
    {
        Assert.ThrowsException<SimulationEngineException>(
            () => new SimulationEngineRegistry(Array.Empty<IMatchSimulationEngine>()));
    }

    private static SimulationEngineRegistry CreateRegistryWithBothEngines()
    {
        return new SimulationEngineRegistry(new IMatchSimulationEngine[]
        {
            new ProbabilisticSimulationEngine(),
            new PureRandomSimulationEngine()
        });
    }
}
