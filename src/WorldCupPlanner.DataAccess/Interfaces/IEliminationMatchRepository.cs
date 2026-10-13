using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IEliminationMatchRepository
{
    Match? GetById(int id);
    IReadOnlyCollection<Match> GetByPhase(Phase phase);
    IReadOnlyCollection<Match> GetAll();
    // Los repositorios EF lo resuelven con un EXISTS; el default sirve a los dobles de test.
    bool Any() => GetAll().Count > 0;
    void Add(Match match);
    void Update(Match match);
    void Delete(Match match);
    bool ExistsById(int id);
    void Clear();
}