using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IEliminationMatchRepository
{
    Match? GetById(int id);
    IReadOnlyCollection<Match> GetByPhase(Phase phase);
    IReadOnlyCollection<Match> GetAll();
    // Los repositorios EF lo resuelven con un EXISTS; el default sirve a los dobles de test.
    bool Any() => GetAll().Count > 0;
    // Id mas alto guardado, null si no hay eliminatorias. EF: un MAX.
    int? MaxMatchId() => GetAll().Max(match => (int?)match.MatchId);
    // Ids de los partidos que ya son fuente (MatchHome/MatchAway) de una ronda siguiente. EF: proyecta solo las dos FK.
    IReadOnlySet<int> GetSourceMatchIds() => GetAll()
        .OfType<MatchElimination>()
        .SelectMany(next => new[] { next.MatchHome?.MatchId, next.MatchAway?.MatchId })
        .OfType<int>()
        .ToHashSet();
    void Add(Match match);
    void Update(Match match);
    void Delete(Match match);
    bool ExistsById(int id);
    void Clear();
}