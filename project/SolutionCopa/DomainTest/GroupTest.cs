using ProjectCopa;
namespace DomainTest;

[TestClass]
public class GroupTest
{
    [TestMethod]
    public void GroupExposesIdProvidedInConstructor()
    {
        Group group = new Group(1, "A");

        Assert.AreEqual(1, group.Id);
    }

    [TestMethod]
    public void GroupRejectsNonPositiveId()
    {
        Assert.ThrowsException<ArgumentException>(() => new Group(0, "A"));
    }

    [TestMethod]
    public void GroupCanBeCreatedWithName()
    {
        Group group = new Group(1, "A");

        Assert.AreEqual("A", group.Name);
        Assert.AreEqual(1, group.Id);
        Assert.AreEqual(0, group.Teams.Count);
    }
    
    
    [TestMethod]
    public void CanAddTeamToGroup()
    {
        Group group = new Group(1, "A");

        ProjectCopa.Team team = new ProjectCopa.Team(
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
        Group group = new Group(1, "A");

        group.AddTeam(new ProjectCopa.Team(1, "T1", 1000, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team(2, "T2", 1100, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team(3, "T3", 1200, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team(4, "T4", 1300, false, Confederation.UEFA));
        group.AddTeam(new ProjectCopa.Team(5, "T5", 1400, false, Confederation.UEFA));
    }

    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GroupCannotContainDuplicateTeams()
    {
        Group group = new Group(1, "A");

        ProjectCopa.Team team = new ProjectCopa.Team(
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
