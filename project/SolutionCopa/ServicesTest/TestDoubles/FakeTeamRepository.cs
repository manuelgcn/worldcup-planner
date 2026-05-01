using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeTeamRepository : ITeamRepository
{
    private readonly Dictionary<int, ITeam> _teams = new();

    public void Set(int id, ITeam team)
    {
        _teams[id] = team;
    }

    public ITeam? GetById(int teamId)
    {
        ITeam? team;
        var found = _teams.TryGetValue(teamId, out team);
        if (!found)
        {
            return null;
        }

        return team;
    }
}
