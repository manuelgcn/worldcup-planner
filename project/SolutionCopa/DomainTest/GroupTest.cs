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
    
    
    [TestMethod]
    public void CanAddTeamToGroup()
    {
        Group group = new Group("A");

        ProjectCopa.Team team = new ProjectCopa.Team(
            "Uruguay",
            1500,
            false,
            Confederation.CONMEBOL
        );

        group.AddTeam(team);

        Assert.AreEqual(1, group.Teams.Count);
        Assert.IsTrue(group.Teams.Contains(team));
    }

}