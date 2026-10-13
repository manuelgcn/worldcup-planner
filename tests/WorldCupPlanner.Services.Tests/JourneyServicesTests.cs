using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class JourneyServicesTests
{
    private MockJourneyRepository _repo = null!;
    private JourneyServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new MockJourneyRepository();
        _service = new JourneyServices(_repo, new MockAuditLogService());
    }

    [TestMethod]
    public void AddJourneyCallsRepository()
    {
        Journey journey = CreateJourney(1);

        _service.AddJourney(journey);

        Assert.AreEqual(1, _repo.AddCalls);
    }

    [TestMethod]
    public void CannotAddJourneyWithDuplicateNumber()
    {
        _service.AddJourney(CreateJourney(1));

        Assert.ThrowsException<JourneyServicesException>(() =>
            _service.AddJourney(CreateJourney(1)));
    }

    [TestMethod]
    public void GetByNumberThrowsWhenJourneyNotFound()
    {
        Assert.ThrowsException<JourneyServicesException>(() => _service.GetByNumber(999));
    }

    [TestMethod]
    public void JourneyServices_RemoveByNumberDeletesViaRepository()
    {
        _service.AddJourney(CreateJourney(1));

        _service.RemoveByNumber(1);

        Assert.AreEqual(1, _repo.DeleteCalls);
    }

    [TestMethod]
    public void GetAllJourneysReturnsStoredJourneys()
    {
        Journey firstJourney = CreateJourney(1);
        Journey secondJourney = CreateJourney(2);
        _service.AddJourney(firstJourney);
        _service.AddJourney(secondJourney);

        IReadOnlyCollection<Journey> journeys = _service.GetAllJourneys();

        Assert.AreEqual(2, journeys.Count);
        Assert.IsTrue(journeys.Contains(firstJourney));
        Assert.IsTrue(journeys.Contains(secondJourney));
    }

    private static Journey CreateJourney(int number)
    {
        return Journey.CreateInstance(number);
    }
}
