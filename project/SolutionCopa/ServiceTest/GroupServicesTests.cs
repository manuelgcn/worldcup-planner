using ProjectCopa;
using Services;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public sealed class GroupServicesTests
{
    private MockGroupRepository _repo = null!;
    private MockUnitOfWork _uow = null!;
    private GroupServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new MockGroupRepository();
        _uow = new MockUnitOfWork();
        _service = new GroupServices(_repo, _uow);
    }

    [TestMethod]
    public void AddGroupCallsRepositoryAndCommits()
    {
        Group group = new Group(1, "A");

        _service.AddGroup(group);

        Assert.AreEqual(1, _repo.AddCalls);
        Assert.AreEqual(1, _uow.CommitCalls);
    }
}
