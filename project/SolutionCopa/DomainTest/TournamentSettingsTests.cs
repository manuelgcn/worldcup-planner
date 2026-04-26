using ProjectCopa;

namespace DomainTest;

[TestClass]
public sealed class TournamentSettingsTests
{
    private TournamentSettings _setting = null!;

    [TestInitialize]
    public void Setup()
    {
        _setting = new TournamentSettings();
    }

    [TestMethod]
    public void SettingsCanBeCreatedWithoutData()
    {
        Assert.IsNotNull(_setting);
    }

    [TestMethod]
    public void SettingsHasDefaultStartDate()
    {
        Assert.AreEqual(new DateTime(2026, 6, 1), _setting.StartDate);
    }
}
