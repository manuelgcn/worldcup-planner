using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly List<Team> _teams = new List<Team>();

    public void Add(Team team)
    {
        _teams.Add(team);
    }

    public void Update(Team team)
    {
        int index = _teams.FindIndex(existingTeam => existingTeam.Id == team.Id);

        if (index >= 0)
        {
            _teams[index] = team;
            return;
        }

        _teams.Add(team);
    }

    public void Delete(Team team)
    {
        _teams.RemoveAll(existingTeam => existingTeam.Id == team.Id);
    }

    public Team? GetById(int teamId)
    {
        return _teams.FirstOrDefault(team => team.Id == teamId);
    }

    public Team? GetByName(string name)
    {
        return _teams.FirstOrDefault(team => team.Name == name);
    }

    public bool ExistsByName(string name)
    {
        return _teams.Any(team => team.Name == name);
    }

    public IReadOnlyCollection<Team> GetAll()
    {
        return _teams.AsReadOnly();
    }

    public int CountByConfederation(Confederation confederation)
    {
        return _teams.Count(team => team.Confederation == confederation);
    }
}
