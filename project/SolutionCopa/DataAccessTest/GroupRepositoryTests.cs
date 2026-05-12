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
    public void AddStoresGroupAndGetByIdReturnsIt()
    {
        Group group = CreateGroup(1, "A");

        _repository.Add(group);

        Group? storedGroup = _repository.GetById(1);
        Assert.AreSame(group, storedGroup);
    }

    [TestMethod]
    public void UpdateReplacesExistingGroupWithSameId()
    {
        Group originalGroup = CreateGroup(2, "B");
        Group updatedGroup = CreateGroup(2, "C");
        _repository.Add(originalGroup);

        _repository.Update(updatedGroup);

        Group? storedGroup = _repository.GetById(2);
        Assert.IsNotNull(storedGroup);
        Assert.AreSame(updatedGroup, storedGroup);
    }

    [TestMethod]
    public void UpdateAddsGroupWhenIdDoesNotExist()
    {
        Group group = CreateGroup(3, "C");

        _repository.Update(group);

        Assert.AreSame(group, _repository.GetById(3));
    }

    [TestMethod]
    public void DeleteRemovesStoredGroup()
    {
        Group group = CreateGroup(4, "D");
        _repository.Add(group);

        _repository.Delete(group);

        Assert.IsNull(_repository.GetById(4));
    }

    [TestMethod]
    public void GetByNameReturnsStoredGroup()
    {
        Group group = CreateGroup(5, "E");
        _repository.Add(group);

        Group? storedGroup = _repository.GetByName("E");

        Assert.AreSame(group, storedGroup);
    }

    [TestMethod]
    public void ExistsByNameReturnsTrueWhenGroupIsStored()
    {
        Group group = CreateGroup(6, "F");
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
        Group firstGroup = CreateGroup(7, "G");
        Group secondGroup = CreateGroup(8, "H");
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

    private static Group CreateGroup(int id, string name)
    {
        return new Group(id, name);
    }
}
