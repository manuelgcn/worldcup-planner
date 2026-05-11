using ProjectCopa;
using Services;
using Services.Exceptions;
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

    [TestMethod]
    public void CannotAddTwoGroupsWithSameName()
    {
        _service.AddGroup(new Group(1, "A"));

        Assert.ThrowsException<GroupServicesException>(() => _service.AddGroup(new Group(2, "A")));
    }

    [TestMethod]
    public void GetByIdThrowsWhenGroupNotFound()
    {
        Assert.ThrowsException<GroupServicesException>(() => _service.GetById(999));
    }

    [TestMethod]
    public void GroupServices_RemoveByIdDeletesViaRepository()
    {
        _service.AddGroup(new Group(1, "A"));

        _service.RemoveById(1);

        Assert.AreEqual(1, _repo.DeleteCalls);
        Assert.AreEqual(2, _uow.CommitCalls);
    }
}
