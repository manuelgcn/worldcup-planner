using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockTeamRepository : ITeamRepository
{
    private readonly Dictionary<int, Team> _teams = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public void Add(Team team)
    {
        _teams[team.Id] = team;
        AddCalls++;
    }

    public void Update(Team team)
    {
        _teams[team.Id] = team;
        UpdateCalls++;
    }

    public void Delete(Team team)
    {
        _teams.Remove(team.Id);
        DeleteCalls++;
    }

    public Team? GetById(int teamId)
    {
        return _teams.GetValueOrDefault(teamId);
    }

    public Team? GetByName(string name)
    {
        return _teams.Values.FirstOrDefault(team => team.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _teams.Values.Any(team => team.Name == name);
    }

    public IReadOnlyCollection<Team> GetAll()
    {
        return _teams.Values.ToList();
    }

    public int CountByConfederation(Confederation confederation)
    {
        return _teams.Values.Count(team => team.Confederation == confederation);
    }
}
