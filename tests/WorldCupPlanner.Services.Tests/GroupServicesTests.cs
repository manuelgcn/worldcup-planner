using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

namespace WorldCupPlanner.Services.Tests;

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
        _service = new GroupServices(_repo, _uow, new MockAuditLogService());
    }

    [TestMethod]
    public void AddGroupCallsRepositoryAndCommits()
    {
        Group group = CreateGroup("A");

        _service.AddGroup(group);

        Assert.AreEqual(1, _repo.AddCalls);
        Assert.AreEqual(1, _uow.CommitCalls);
    }

    [TestMethod]
    public void CannotAddTwoGroupsWithSameName()
    {
        _service.AddGroup(CreateGroup("A"));

        Assert.ThrowsException<GroupServicesException>(() => _service.AddGroup(CreateGroup("A")));
    }

    [TestMethod]
    public void GetByNameThrowsWhenGroupNotFound()
    {
        Assert.ThrowsException<GroupServicesException>(() => _service.GetByName("NoExiste"));
    }

    [TestMethod]
    public void GroupServices_RemoveByNameDeletesViaRepository()
    {
        _service.AddGroup(CreateGroup("A"));

        _service.RemoveByName("A");

        Assert.AreEqual(1, _repo.DeleteCalls);
        Assert.AreEqual(2, _uow.CommitCalls);
    }

    [TestMethod]
    public void GetAllGroupsReturnsStoredGroups()
    {
        Group firstGroup = CreateGroup("A");
        Group secondGroup = CreateGroup("B");
        _service.AddGroup(firstGroup);
        _service.AddGroup(secondGroup);

        IReadOnlyCollection<Group> groups = _service.GetAllGroups();

        Assert.AreEqual(2, groups.Count);
        Assert.IsTrue(groups.Contains(firstGroup));
        Assert.IsTrue(groups.Contains(secondGroup));
    }

    private static Group CreateGroup(string name)
    {
        return new Group(name);
    }
}
