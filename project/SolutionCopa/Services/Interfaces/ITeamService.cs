using ProjectCopa;

namespace Services.Interfaces;

public interface ITeamService
{
    void AddTeam(Team team);
    IReadOnlyCollection<Team> GetTeams();
    Team FindByName(string name);
    void RemoveTeam(string name);
    IReadOnlyList<Team> OrderForFixture(IReadOnlyCollection<Team> teams, int fixtureSeed);
}
