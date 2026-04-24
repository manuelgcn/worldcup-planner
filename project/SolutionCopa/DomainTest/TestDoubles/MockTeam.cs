using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockTeam : ITeam
{
    public int TeamId { get; }
    public string Name { get; }

    public MockTeam(int teamId = 1, string name = "Mock Team")
    {
        TeamId = teamId;
        Name = name;
    }
}

