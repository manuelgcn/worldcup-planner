using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class Team : ITeam
{
    public int TeamId { get; }
    public string Name { get; }

    public Team(int teamId = 0, string name = "")
    {
        TeamId = teamId;
        Name = name;
    }
}
