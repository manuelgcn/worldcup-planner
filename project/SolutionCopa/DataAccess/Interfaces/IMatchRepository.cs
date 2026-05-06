using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IMatchRepository
{
    bool ExistsById(int matchId);
    void Add(Match match);
    void Update(Match match);
    void Delete(Match match);
    Match? GetById(int matchId);
    List<Match> GetByGroup(string groupName);
    List<Match> GetByJourney(int journeyNumber);
    List<Match> GetByDateRange(DateTime from, DateTime to);
}
