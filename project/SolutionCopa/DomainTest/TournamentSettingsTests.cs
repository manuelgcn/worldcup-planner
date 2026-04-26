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
}
