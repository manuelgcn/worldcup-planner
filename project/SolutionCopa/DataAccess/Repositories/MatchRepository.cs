using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly List<Match> _matches = new List<Match>();

    public bool ExistsById(int matchId)
    {
        return _matches.Any(match => match.MatchId == matchId);
    }

    public void Add(Match match)
    {
        _matches.Add(match);
    }

    public void Update(Match match)
    {
        int matchIndex = _matches.FindIndex(currentMatch => currentMatch.MatchId == match.MatchId);

        if (matchIndex >= 0)
        {
            _matches[matchIndex] = match;
            return;
        }

        _matches.Add(match);
    }

    public void Delete(Match match)
    {
        _matches.RemoveAll(currentMatch => currentMatch.MatchId == match.MatchId);
    }

    public Match? GetById(int matchId)
    {
        return _matches.FirstOrDefault(match => match.MatchId == matchId);
    }

    public List<Match> GetByGroup(string groupName)
    {
        return _matches
            .OfType<MatchGroup>()
            .Where(matchGroup => IsGroupMatch(matchGroup, groupName))
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByJourney(int journeyNumber)
    {
        return _matches
            .OfType<MatchGroup>()
            .Where(matchGroup => IsJourneyMatch(matchGroup, journeyNumber))
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByStadium(int stadiumId)
    {
        return _matches
            .Where(match => match.Stadium != null && match.Stadium.Id == stadiumId)
            .ToList();
    }

    public List<Match> GetByPhase(Phase phase)
    {
        return _matches
            .OfType<MatchElimination>()
            .Where(elimination => elimination.Phase == phase)
            .Cast<Match>()
            .ToList();
    }

    public List<Match> GetByDateRange(DateTime from, DateTime to)
    {
        return _matches
            .Where(match => IsDateInRange(match, from, to))
            .ToList();
    }

    public List<Match> GetAll()
    {
        return _matches.ToList();
    }

    private static bool IsGroupMatch(MatchGroup matchGroup, string groupName)
    {
        return matchGroup.Group != null && matchGroup.Group.Name == groupName;
    }

    private static bool IsJourneyMatch(MatchGroup matchGroup, int journeyNumber)
    {
        return matchGroup.Journey != null && matchGroup.Journey.Number == journeyNumber;
    }

    private static bool IsDateInRange(Match match, DateTime from, DateTime to)
    {
        return match.MatchDate >= from && match.MatchDate <= to;
    }
}
