using ProjectCopa;

namespace DomainTest;

[TestClass]
public sealed class TournamentSettingsTests
{
    private TournamentSettings _tournamentSettings = null!;

    [TestInitialize]
    public void InitializeTournamentSettings()
    {
        _tournamentSettings = new TournamentSettings();
    }

    [TestMethod]
    public void ShouldCreateTournamentSettings()
    {
        Assert.IsNotNull(_tournamentSettings);
    }

    [TestMethod]
    public void ShouldHaveDefaultStartDate()
    {
        Assert.AreEqual(new DateTime(2026, 6, 1), _tournamentSettings.StartDate);
    }

    [TestMethod]
    public void ShouldHaveDefaultMaxMatchesPerDay()
    {
        Assert.AreEqual(3, _tournamentSettings.MaxMatchesPerDay);
    }

    [TestMethod]
    public void SettingsHasDefaultDaysBetweenJourneys()
    {
        Assert.AreEqual(3, _tournamentSettings.DaysBetweenJourneys);
    }

    [TestMethod]
    public void SettingsHasDefaultFixtureSeed()
    {
        Assert.AreEqual(0, _tournamentSettings.FixtureSeed);
    }
    
    [TestMethod]
    public void SettingsHasDefaultCrossingSeed()
    {
        Assert.AreEqual(0, _tournamentSettings.CrossingsSeed);
    }
    
    [TestMethod]
    public void SettingsHasDefaultSimulationSeed()
    {
        Assert.AreEqual(0, _tournamentSettings.SimulationSeed);
    }
    
    [TestMethod]
    public void SettingsHasDefaultCompletionSeed()
    {
        Assert.AreEqual(0, _tournamentSettings.CompletionSeed);
    }

    [TestMethod]
    public void SettingsCanGetId()
    {
        // Arrange
        var id = 10;
        var startDate = new DateTime(2026, 6, 1);
        const int maxMatchesPerDay = 3;
        const int daysBetweenJourneys = 3;
        const int fixtureSeed = 0;
        const int crossingsSeed = 0;
        const int simulationSeed = 0;
        const int completionSeed = 0;

        // Act
        var settings = new TournamentSettings(
            id,
            startDate,
            maxMatchesPerDay,
            daysBetweenJourneys,
            fixtureSeed,
            crossingsSeed,
            simulationSeed,
            completionSeed);

        // Assert
        Assert.AreEqual(id, settings.Id);
    }
}
