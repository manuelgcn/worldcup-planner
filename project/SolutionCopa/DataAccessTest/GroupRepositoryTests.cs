using DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

[TestClass]
public class GroupRepositoryTests
{
    private GroupRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new GroupRepository();
    }

    [TestMethod]
    public void AddStoresGroupAndGetByNameReturnsIt()
    {
        Group group = CreateGroup("A");

        _repository.Add(group);

        Group? storedGroup = _repository.GetByName("A");
        Assert.AreSame(group, storedGroup);
    }

    [TestMethod]
    public void UpdateReplacesExistingGroupWithSameName()
    {
        Group originalGroup = CreateGroup("A");
        Group updatedGroup = CreateGroup("A");
        _repository.Add(originalGroup);

        _repository.Update(updatedGroup);

        Group? storedGroup = _repository.GetByName("A");
        Assert.IsNotNull(storedGroup);
        Assert.AreSame(updatedGroup, storedGroup);
    }

    [TestMethod]
    public void UpdateAddsGroupWhenNameDoesNotExist()
    {
        Group group = CreateGroup("C");

        _repository.Update(group);

        Assert.AreSame(group, _repository.GetByName("C"));
    }

    [TestMethod]
    public void DeleteRemovesStoredGroup()
    {
        Group group = CreateGroup("D");
        _repository.Add(group);

        _repository.Delete(group);

        Assert.IsNull(_repository.GetByName("D"));
    }

    [TestMethod]
    public void GetByNameReturnsStoredGroup()
    {
        Group group = CreateGroup("E");
        _repository.Add(group);

        Group? storedGroup = _repository.GetByName("E");

        Assert.AreSame(group, storedGroup);
    }

    [TestMethod]
    public void ExistsByNameReturnsTrueWhenGroupIsStored()
    {
        Group group = CreateGroup("F");
        _repository.Add(group);

        bool exists = _repository.ExistsByName("F");

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void ExistsByNameReturnsFalseWhenGroupIsNotStored()
    {
        bool exists = _repository.ExistsByName("Z");

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void GetAllReturnsAllStoredGroups()
    {
        Group firstGroup = CreateGroup("G");
        Group secondGroup = CreateGroup("H");
        _repository.Add(firstGroup);
        _repository.Add(secondGroup);

        IReadOnlyCollection<Group> groups = _repository.GetAll();

        Assert.AreEqual(2, groups.Count);
        Assert.IsTrue(groups.Contains(firstGroup));
        Assert.IsTrue(groups.Contains(secondGroup));
    }

    [TestMethod]
    public void AddStoresGroupAndGetByNameReturnsIt_NameOnly()
    {
        Group group = new Group("A");

        _repository.Add(group);

        Group? stored = _repository.GetByName("A");
        Assert.AreSame(group, stored);
    }

    private static Group CreateGroup(string name)
    {
        return new Group(name);
    }
}
