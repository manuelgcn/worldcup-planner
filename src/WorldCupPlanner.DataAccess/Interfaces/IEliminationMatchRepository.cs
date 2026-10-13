using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IEliminationMatchRepository
{
    Match? GetById(int id);
    IReadOnlyCollection<Match> GetByPhase(Phase phase);
    IReadOnlyCollection<Match> GetAll();
    void Add(Match match);
    void Update(Match match);
    void Delete(Match match);
    bool ExistsById(int id);
    void Clear();
}