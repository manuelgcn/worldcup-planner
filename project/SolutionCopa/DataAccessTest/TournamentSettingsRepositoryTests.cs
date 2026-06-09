using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
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
        // Contextos efimeros sobre una BD InMemory aislada por test (mismo patron que TeamRepositoryTests).
        _repository = new TournamentSettingsRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresSettingsAndGetByIdReturnsIt()
    {
        TournamentSettings settings = CreateSettings(id: 1);

        _repository.Add(settings);

        TournamentSettings? storedSettings = _repository.GetById(1);
        Assert.IsNotNull(storedSettings);
        AssertSameValues(settings, storedSettings!);
    }

    [TestMethod]
    public void UpdateReplacesExistingSettingsWithSameId()
    {
        TournamentSettings originalSettings = CreateSettings(id: 2);
        TournamentSettings updatedSettings = CreateSettings(id: 2, maxMatchesPerDay: 2, daysBetweenJourneys: 1);
        _repository.Add(originalSettings);

        _repository.Update(updatedSettings);

        TournamentSettings? storedSettings = _repository.GetById(2);
        Assert.IsNotNull(storedSettings);
        Assert.AreEqual(2, storedSettings!.MaxMatchesPerDay);
        Assert.AreEqual(1, storedSettings.DaysBetweenJourneys);
        AssertSameValues(updatedSettings, storedSettings);
    }

    [TestMethod]
    public void UpdateAddsSettingsWhenIdDoesNotExist()
    {
        TournamentSettings settings = CreateSettings(id: 3);

        _repository.Update(settings);

        TournamentSettings? storedSettings = _repository.GetById(3);
        Assert.IsNotNull(storedSettings);
        AssertSameValues(settings, storedSettings!);
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
        AssertSameValues(existingSettings, storedSettings!);
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
        Assert.IsTrue(allSettings.Any(s => s.Id == 6));
        Assert.IsTrue(allSettings.Any(s => s.Id == 7));
    }

    [TestMethod]
    public void GetAllReturnsLocalCopyAndChangesDoNotAffectRepository()
    {
        TournamentSettings settings = CreateSettings(id: 8);
        _repository.Add(settings);

        List<TournamentSettings> localCopy = _repository.GetAll().ToList();
        localCopy.Clear();

        Assert.AreEqual(expected: 1, actual: _repository.GetAll().Count);
    }

    [TestMethod]
    public void AddPersistsSettingsRetrievableFromNewContext()
    {
        TournamentSettings settings = CreateSettings(
            id: 12,
            startDate: new DateTime(2026, 7, 2),
            maxMatchesPerDay: 2,
            daysBetweenJourneys: 4,
            fixtureSeed: 11,
            crossingsSeed: 22,
            simulationSeed: 33,
            completionSeed: 44);

        _repository.Add(settings);                              // SaveChanges interno (autosave por operacion)

        TournamentSettings? stored = _repository.GetById(12);   // contexto nuevo: el dato viene de la "BD"
        Assert.IsNotNull(stored);
        AssertSameValues(settings, stored!);
    }

    [TestMethod]
    public void AddPersistsSelectedSimulationEngineRetrievableFromNewContext()
    {
        TournamentSettings settings = CreateSettings(id: 13);
        settings.SelectSimulationEngine("Aleatorio Puro");

        _repository.Add(settings);

        TournamentSettings? stored = _repository.GetById(13);
        Assert.IsNotNull(stored);
        Assert.AreEqual("Aleatorio Puro", stored!.SelectedSimulationEngine);
    }

    [TestMethod]
    public void UpdatePersistsSelectedSimulationEngineChange()
    {
        TournamentSettings settings = CreateSettings(id: 14);
        _repository.Add(settings);

        settings.SelectSimulationEngine("Aleatorio Puro");
        _repository.Update(settings);

        TournamentSettings? stored = _repository.GetById(14);
        Assert.IsNotNull(stored);
        Assert.AreEqual("Aleatorio Puro", stored!.SelectedSimulationEngine);
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

    private static void AssertSameValues(TournamentSettings expected, TournamentSettings actual)
    {
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.StartDate, actual.StartDate);
        Assert.AreEqual(expected.MaxMatchesPerDay, actual.MaxMatchesPerDay);
        Assert.AreEqual(expected.DaysBetweenJourneys, actual.DaysBetweenJourneys);
        Assert.AreEqual(expected.FixtureSeed, actual.FixtureSeed);
        Assert.AreEqual(expected.CrossingsSeed, actual.CrossingsSeed);
        Assert.AreEqual(expected.SimulationSeed, actual.SimulationSeed);
        Assert.AreEqual(expected.CompletionSeed, actual.CompletionSeed);
        Assert.AreEqual(expected.SelectedSimulationEngine, actual.SelectedSimulationEngine);
    }
}
