using DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class TournamentSettingsRepositoryTests
{
    private TournamentSettingsRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new TournamentSettingsRepository();
    }

    [TestMethod]
    public void AddStoresSettingsAndGetByIdReturnsIt()
    {
        TournamentSettings settings = CreateSettings(id: 1);

        _repository.Add(settings);

        TournamentSettings? storedSettings = _repository.GetById(1);
        Assert.AreSame(settings, storedSettings);
    }

    [TestMethod]
    public void UpdateReplacesExistingSettingsWithSameId()
    {
        TournamentSettings originalSettings = CreateSettings(id: 2, maxMatchesPerDay: 3, daysBetweenJourneys: 3);
        TournamentSettings updatedSettings = CreateSettings(id: 2, maxMatchesPerDay: 2, daysBetweenJourneys: 1);
        _repository.Add(originalSettings);

        _repository.Update(updatedSettings);

        TournamentSettings? storedSettings = _repository.GetById(2);
        Assert.IsNotNull(storedSettings);
        Assert.AreSame(updatedSettings, storedSettings);
    }

    [TestMethod]
    public void UpdateAddsSettingsWhenIdDoesNotExist()
    {
        TournamentSettings settings = CreateSettings(id: 3);

        _repository.Update(settings);

        TournamentSettings? storedSettings = _repository.GetById(3);
        Assert.AreSame(settings, storedSettings);
    }

    [TestMethod]
    public void DeleteRemovesStoredSettings()
    {
        TournamentSettings settings = CreateSettings(id: 4);
        _repository.Add(settings);

        _repository.Delete(settings);

        TournamentSettings? storedSettings = _repository.GetById(4);
        Assert.IsNull(storedSettings);
    }

    [TestMethod]
    public void DeleteDoesNothingWhenSettingsIsNotPresent()
    {
        TournamentSettings existingSettings = CreateSettings(id: 5);
        TournamentSettings nonExistingSettings = CreateSettings(id: 99);
        _repository.Add(existingSettings);

        _repository.Delete(nonExistingSettings);

        TournamentSettings? storedSettings = _repository.GetById(5);
        Assert.IsNotNull(storedSettings);
    }

    [TestMethod]
    public void GetByIdReturnsNullWhenSettingsDoesNotExist()
    {
        TournamentSettings? settings = _repository.GetById(999);

        Assert.IsNull(settings);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredSettings()
    {
        TournamentSettings firstSettings = CreateSettings(id: 6);
        TournamentSettings secondSettings = CreateSettings(id: 7, maxMatchesPerDay: 1, daysBetweenJourneys: 2);
        _repository.Add(firstSettings);
        _repository.Add(secondSettings);

        IReadOnlyCollection<TournamentSettings> allSettings = _repository.GetAll();

        Assert.AreEqual(expected: 2, actual: allSettings.Count);
        Assert.IsTrue(allSettings.Contains(firstSettings));
        Assert.IsTrue(allSettings.Contains(secondSettings));
    }

    [TestMethod]
    public void GetAllReturnsReadOnlyCollectionAndLocalCopyChangesDoNotAffectRepository()
    {
        TournamentSettings settings = CreateSettings(id: 8);
        _repository.Add(settings);

        IReadOnlyCollection<TournamentSettings> allSettings = _repository.GetAll();
        List<TournamentSettings> localCopy = allSettings.ToList();
        localCopy.Clear();

        Assert.AreEqual(expected: 1, actual: _repository.GetAll().Count);
    }

    private static TournamentSettings CreateSettings(
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
            startDate ?? new DateTime(2026, 6, 10),
            maxMatchesPerDay,
            daysBetweenJourneys,
            fixtureSeed,
            crossingsSeed,
            simulationSeed,
            completionSeed);
    }
}
