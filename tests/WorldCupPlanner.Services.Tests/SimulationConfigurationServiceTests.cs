using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class SimulationConfigurationServiceTests
{
    private MockTournamentSettingsRepository _settingsRepository = null!;
    private ISimulationConfigurationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _settingsRepository = new MockTournamentSettingsRepository();
        _service = new SimulationConfigurationService(_settingsRepository);
    }

    [TestMethod]
    public void GetConfiguration_WhenSettingsExist_ReturnsEngineAndSeed()
    {
        TournamentSettings settings = new TournamentSettings(1, new DateTime(2026, 6, 1), 3, 3, 0, 0, 42, 0);
        settings.SelectSimulationEngine("Aleatorio puro");
        _settingsRepository.Add(settings);

        SimulationConfigurationDto? configuration = _service.GetConfiguration();

        Assert.IsNotNull(configuration);
        Assert.AreEqual("Aleatorio puro", configuration!.SelectedSimulationEngine);
        Assert.AreEqual(42, configuration.SimulationSeed);
    }

    [TestMethod]
    public void GetConfiguration_WhenNoSettings_ReturnsNull()
    {
        SimulationConfigurationDto? configuration = _service.GetConfiguration();

        Assert.IsNull(configuration);
    }
}
