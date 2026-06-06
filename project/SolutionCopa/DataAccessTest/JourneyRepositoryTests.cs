using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class JourneyRepositoryTests
{
    private JourneyRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Contextos efimeros sobre una BD InMemory aislada por test.
        _repository = new JourneyRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresJourneyAndGetByNumberReturnsIt()
    {
        _repository.Add(Journey.CreateInstance(1));

        Journey? stored = _repository.GetByNumber(1);

        Assert.IsNotNull(stored);
        Assert.AreEqual(1, stored!.Number);
    }

    [TestMethod]
    public void AddPersistsJourneyRetrievableFromNewContext()
    {
        _repository.Add(Journey.CreateInstance(2));     // SaveChanges interno (autosave por operacion)

        Journey? stored = _repository.GetByNumber(2);   // contexto nuevo: el dato viene de la "BD"

        Assert.IsNotNull(stored);
        Assert.AreEqual(2, stored!.Number);
    }

    [TestMethod]
    public void UpdateAddsJourneyWhenNumberDoesNotExist()
    {
        _repository.Update(Journey.CreateInstance(2));

        Assert.IsTrue(_repository.ExistsByNumber(2));
    }

    [TestMethod]
    public void UpdateKeepsJourneyWhenNumberAlreadyExists()
    {
        _repository.Add(Journey.CreateInstance(1));

        _repository.Update(Journey.CreateInstance(1));

        Assert.AreEqual(1, _repository.GetAll().Count);
    }

    [TestMethod]
    public void DeleteRemovesStoredJourney()
    {
        _repository.Add(Journey.CreateInstance(3));

        _repository.Delete(Journey.CreateInstance(3));

        Assert.IsNull(_repository.GetByNumber(3));
    }

    [TestMethod]
    public void ExistsByNumberReturnsTrueWhenJourneyIsStored()
    {
        _repository.Add(Journey.CreateInstance(4));

        Assert.IsTrue(_repository.ExistsByNumber(4));
    }

    [TestMethod]
    public void ExistsByNumberReturnsFalseWhenJourneyIsNotStored()
    {
        Assert.IsFalse(_repository.ExistsByNumber(999));
    }

    [TestMethod]
    public void GetAllReturnsAllStoredJourneys()
    {
        _repository.Add(Journey.CreateInstance(5));
        _repository.Add(Journey.CreateInstance(6));

        IReadOnlyCollection<Journey> journeys = _repository.GetAll();

        Assert.AreEqual(2, journeys.Count);
        Assert.IsTrue(journeys.Any(journey => journey.Number == 5));
        Assert.IsTrue(journeys.Any(journey => journey.Number == 6));
    }

    [TestMethod]
    public void ClearRemovesAllStoredJourneys()
    {
        _repository.Add(Journey.CreateInstance(1));
        _repository.Add(Journey.CreateInstance(2));

        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }
}
