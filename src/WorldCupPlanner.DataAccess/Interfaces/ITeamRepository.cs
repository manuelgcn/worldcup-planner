using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface ITeamRepository
{
    void Add(Team team);
    void Update(Team team);
    void Delete(Team team);
    Team? GetById(int teamId);
    Team? GetByName(string name);
    bool ExistsByName(string name);
    IReadOnlyCollection<Team> GetAll();
    int CountByConfederation(Confederation confederation);
}
