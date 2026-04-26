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

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GroupCannotHaveMoreThanFourTeams()
    {
        Group group = new Group("A");

        group.AddTeam(new ProjectCopa.Team("T1", 1000, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team("T2", 1100, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team("T3", 1200, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team("T4", 1300, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team("T5", 1400, false, Confederation.UEFA));
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GroupCannotContainDuplicateTeams()
    {
        Group group = new Group("A");

        ProjectCopa.Team team = new ProjectCopa.Team(
            "Uruguay",
            1500,
            false,
            Confederation.CONMEBOL
        );

        group.AddTeam(team);
        group.AddTeam(team); 
    }

    
    
}