using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class StubTeam : ITeam
{
    public int TeamId { get; }
    public string Name { get; }

    public StubTeam(int teamId = 1, string name = "Team")
    {
        TeamId = teamId;
        Name = name;
    }
}
