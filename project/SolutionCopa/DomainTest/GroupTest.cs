using ProjectCopa;

namespace DomainTest;

[TestClass]
public class GroupTest
{
    [TestMethod]
    public void GroupCanBeCreatedWithName()
    {
        Group group = new Group("A");

        Assert.AreEqual("A", group.Name);
        Assert.AreEqual(0, group.Teams.Count);
    }
}