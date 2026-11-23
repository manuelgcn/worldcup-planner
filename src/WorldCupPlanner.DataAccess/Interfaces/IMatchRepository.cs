using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Interfaces;

public interface IMatchRepository
{
    bool ExistsById(int matchId);
    void Add(Match match);
    void Update(Match match);
    void Delete(Match match);
    Match? GetById(int matchId);
    List<Match> GetByGroup(string groupName);
    List<Match> GetByJourney(int journeyNumber);
    List<Match> GetByStadium(int stadiumId);
    List<Match> GetByPhase(Phase phase);
    List<Match> GetByDateRange(DateTime from, DateTime to);
    List<Match> Search(MatchQueryCriteria criteria);
    List<Match> GetAll();
    // Los repositorios EF resuelven estas tres con un COUNT/MAX; el default sirve a los dobles de test.
    int Count() => GetAll().Count;
    int CountPlayed() => GetAll().Count(match => match.HasResult);
    // Fecha del ultimo partido de grupos; null si no hay ninguno.
    DateTime? LatestGroupMatchDate() => GetAll().OfType<MatchGroup>().Max(match => (DateTime?)match.MatchDate);
    int NextIncidentId();
    void Clear();
}
