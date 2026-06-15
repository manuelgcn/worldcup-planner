using ProjectCopa;
using ProjectCopa.Exceptions;

namespace DomainTest;

[TestClass]
public sealed class TournamentSettingsSimulationEngineTests
{
    private const string DefaultEngineName = "Probabilístico";
    private const string PureRandomEngineName = "Aleatorio Puro";

    [TestMethod]
    public void NewTournamentSettings_DefaultsSelectedSimulationEngineToProbabilistico()
    {
        TournamentSettings settings = new TournamentSettings();

        Assert.AreEqual(DefaultEngineName, settings.SelectedSimulationEngine);
    }

    [TestMethod]
    public void NewTournamentSettings_WithFullConstructor_DefaultsSelectedSimulationEngineToProbabilistico()
    {
        TournamentSettings settings = new TournamentSettings(
            1, new DateTime(2026, 6, 1), 3, 3, 0, 0, 0, 0);

        Assert.AreEqual(DefaultEngineName, settings.SelectedSimulationEngine);
    }

    [TestMethod]
    public void SelectSimulationEngine_WithValidName_UpdatesSelectedSimulationEngine()
    {
        TournamentSettings settings = new TournamentSettings();

        settings.SelectSimulationEngine(PureRandomEngineName);

        Assert.AreEqual(PureRandomEngineName, settings.SelectedSimulationEngine);
    }

    [TestMethod]
    public void SelectSimulationEngine_WithNull_ThrowsSettingsException()
    {
        TournamentSettings settings = new TournamentSettings();

        Assert.ThrowsException<SettingsException>(() => settings.SelectSimulationEngine(null!));
    }

    [TestMethod]
    public void SelectSimulationEngine_WithEmptyString_ThrowsSettingsException()
    {
        TournamentSettings settings = new TournamentSettings();

        Assert.ThrowsException<SettingsException>(() => settings.SelectSimulationEngine(string.Empty));
    }

    [TestMethod]
    public void SelectSimulationEngine_WithWhitespace_ThrowsSettingsException()
    {
        TournamentSettings settings = new TournamentSettings();

        Assert.ThrowsException<SettingsException>(() => settings.SelectSimulationEngine("   "));
    }
}
