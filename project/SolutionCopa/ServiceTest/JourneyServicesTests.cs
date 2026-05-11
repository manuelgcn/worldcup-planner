using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class JourneyServicesTests
{
    private MockJourneyRepository _repo = null!;
    private MockUnitOfWork _uow = null!;
    private JourneyServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new MockJourneyRepository();
        _uow = new MockUnitOfWork();
        _service = new JourneyServices(_repo, _uow);
    }

    [TestMethod]
    public void AddJourneyCallsRepositoryAndCommits()
    {
        Journey journey = Journey.CreateInstance(1);

        _service.AddJourney(journey);

        Assert.AreEqual(1, _repo.AddCalls);
        Assert.AreEqual(1, _uow.CommitCalls);
    }
}
