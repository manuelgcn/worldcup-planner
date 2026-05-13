using DataAccess.Repositories;
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
        _repository = new JourneyRepository();
    }

    [TestMethod]
    public void AddStoresJourneyAndGetByNumberReturnsIt()
    {
        Journey journey = Create(1);

        _repository.Add(journey);

        Journey? storedJourney = _repository.GetByNumber(1);
        Assert.AreSame(journey, storedJourney);
    }

    [TestMethod]
    public void UpdateReplacesExistingJourneyWithSameNumber()
    {
        Journey originalJourney = Create(1);
        Journey updatedJourney = Create(1);
        _repository.Add(originalJourney);

        _repository.Update(updatedJourney);

        Journey? storedJourney = _repository.GetByNumber(1);
        Assert.IsNotNull(storedJourney);
        Assert.AreSame(updatedJourney, storedJourney);
    }

    [TestMethod]
    public void UpdateAddsJourneyWhenNumberDoesNotExist()
    {
        Journey journey = Create(2);

        _repository.Update(journey);

        Assert.AreSame(journey, _repository.GetByNumber(2));
    }

    [TestMethod]
    public void DeleteRemovesStoredJourney()
    {
        Journey journey = Create(3);
        _repository.Add(journey);

        _repository.Delete(journey);

        Assert.IsNull(_repository.GetByNumber(3));
    }

    [TestMethod]
    public void ExistsByNumberReturnsTrueWhenJourneyIsStored()
    {
        Journey journey = Create(4);
        _repository.Add(journey);

        bool exists = _repository.ExistsByNumber(4);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void ExistsByNumberReturnsFalseWhenJourneyIsNotStored()
    {
        bool exists = _repository.ExistsByNumber(999);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredJourneys()
    {
        Journey firstJourney = Create(5);
        Journey secondJourney = Create(6);
        _repository.Add(firstJourney);
        _repository.Add(secondJourney);

        IReadOnlyCollection<Journey> journeys = _repository.GetAll();

        Assert.AreEqual(2, journeys.Count);
        Assert.IsTrue(journeys.Contains(firstJourney));
        Assert.IsTrue(journeys.Contains(secondJourney));
    }

    [TestMethod]
    public void ClearRemovesAllStoredJourneys()
    {
        _repository.Add(Create(1));
        _repository.Add(Create(2));

        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    [TestMethod]
    public void ClearOnEmptyRepositoryDoesNotThrow()
    {
        _repository.Clear();

        Assert.AreEqual(0, _repository.GetAll().Count);
    }

    private static Journey Create(int number)
    {
        return Journey.CreateInstance(number);
    }
}
