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

        Team team = new Team(
            1,
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

        group.AddTeam(new Team(1, "T1", 1000, false, Confederation.UEFA));
        group.AddTeam(new Team(2, "T2", 1100, false, Confederation.UEFA));
        group.AddTeam(new Team(3, "T3", 1200, false, Confederation.UEFA));
        group.AddTeam(new Team(4, "T4", 1300, false, Confederation.UEFA));
        group.AddTeam(new Team(5, "T5", 1400, false, Confederation.UEFA));
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GroupCannotContainDuplicateTeams()
    {
        Group group = new Group("A");

        Team team = new Team(
            1,
            "Uruguay",
            1500,
            false,
            Confederation.CONMEBOL
        );

        group.AddTeam(team);
        group.AddTeam(team); 
    }

    
    
}
