using ProjectCopa;
using ProjectCopa.Exceptions;

namespace DomainTest;

[TestClass]
public sealed class TournamentSettingsTests
{
    private TournamentSettings _tournamentSettings = null!;
    private static readonly DateTime BaseParameterizedStartDate = new DateTime(2026, 6, 10);

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

        // Act
        var settings = CreateConfiguredSettings(id: id);

        // Assert
        Assert.AreEqual(id, settings.Id);
    }

    [TestMethod]
    public void SettingsCanGetStartDate()
    {
        // Arrange
        var startDate = new DateTime(2026, 6, 10);

        // Act
        var settings = CreateConfiguredSettings(startDate: startDate);

        // Assert
        Assert.AreEqual(startDate, settings.StartDate);
    }

    [TestMethod]
    public void SettingsCanGetMaxMatchesPerDay()
    {
        // Arrange
        const int maxMatchesPerDay = 3;

        // Act
        var settings = CreateConfiguredSettings(maxMatchesPerDay: maxMatchesPerDay);

        // Assert
        Assert.AreEqual(maxMatchesPerDay, settings.MaxMatchesPerDay);
    }

    [TestMethod]
    public void SettingsCanGetDaysBetweenJourneys()
    {
        // Arrange
        const int daysBetweenJourneys = 4;

        // Act
        var settings = CreateConfiguredSettings(daysBetweenJourneys: daysBetweenJourneys);

        // Assert
        Assert.AreEqual(daysBetweenJourneys, settings.DaysBetweenJourneys);
    }

    [TestMethod]
    public void SettingsCanGetFixtureSeed()
    {
        // Arrange
        const int fixtureSeed = 11;

        // Act
        var settings = CreateConfiguredSettings(fixtureSeed: fixtureSeed);

        // Assert
        Assert.AreEqual(fixtureSeed, settings.FixtureSeed);
    }

    [TestMethod]
    public void SettingsCanGetCrossingsSeed()
    {
        // Arrange
        const int crossingsSeed = 22;

        // Act
        var settings = CreateConfiguredSettings(crossingsSeed: crossingsSeed);

        // Assert
        Assert.AreEqual(crossingsSeed, settings.CrossingsSeed);
    }

    [TestMethod]
    public void SettingsCanGetSimulationSeed()
    {
        // Arrange
        const int simulationSeed = 33;

        // Act
        var settings = CreateConfiguredSettings(simulationSeed: simulationSeed);

        // Assert
        Assert.AreEqual(simulationSeed, settings.SimulationSeed);
    }

    [TestMethod]
    public void SettingsCanGetCompletionSeed()
    {
        // Arrange
        const int completionSeed = 44;

        // Act
        var settings = CreateConfiguredSettings(completionSeed: completionSeed);

        // Assert
        Assert.AreEqual(completionSeed, settings.CompletionSeed);
    }

    [TestMethod]
    public void SettingsRejectsZeroMaxMatchesPerDay()
    {
        // Act
        void Action() => CreateConfiguredSettings(maxMatchesPerDay: 0);

        // Assert
        Assert.ThrowsException<SettingsException>(Action);
    }

    [TestMethod]
    public void SettingsRejectsNegativeMaxMatchesPerDay()
    {
        // Act
        void Action() => CreateConfiguredSettings(maxMatchesPerDay: -1);

        // Assert
        Assert.ThrowsException<SettingsException>(Action);
    }

    [TestMethod]
    public void SettingsRejectsMoreThanThreeMatchesPerDay()
    {
        // Act
        void Action() => CreateConfiguredSettings(maxMatchesPerDay: 4);

        // Assert
        Assert.ThrowsException<SettingsException>(Action);
    }

    [TestMethod]
    public void SettingsRejectsZeroDaysBetweenJourneys()
    {
        // Act
        void Action() => CreateConfiguredSettings(daysBetweenJourneys: 0);

        // Assert
        Assert.ThrowsException<SettingsException>(Action);
    }

    private static TournamentSettings CreateConfiguredSettings(
        int id = 10,
        DateTime? startDate = null,
        int maxMatchesPerDay = 3,
        int daysBetweenJourneys = 3,
        int fixtureSeed = 0,
        int crossingsSeed = 0,
        int simulationSeed = 0,
        int completionSeed = 0)
    {
        return new TournamentSettings(
            id,
            startDate ?? BaseParameterizedStartDate,
            maxMatchesPerDay,
            daysBetweenJourneys,
            fixtureSeed,
            crossingsSeed,
            simulationSeed,
            completionSeed);
    }
}
