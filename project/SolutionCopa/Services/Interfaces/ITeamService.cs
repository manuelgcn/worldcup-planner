using ProjectCopa;

namespace Services.Interfaces;

public interface ITeamService
{
    void AddTeam(Team team, string? performedByEmail = null);
    void FillToFortyEight(int? completionSeed, string? performedByEmail);
    IReadOnlyCollection<Team> GetTeams();
    Team FindByName(string name);
    void RemoveTeam(string name, string? performedByEmail = null);
    IReadOnlyList<Team> OrderForFixture(IReadOnlyCollection<Team> teams, int fixtureSeed);
}
