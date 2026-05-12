using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockMatchRepository : IMatchRepository
{
    private readonly Dictionary<int, Match> _matches = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public bool ExistsById(int matchId)
    {
        return _matches.ContainsKey(matchId);
    }

    public void Add(Match match)
    {
        _matches[match.MatchId] = match;
        AddCalls++;
    }

    public void Update(Match match)
    {
        _matches[match.MatchId] = match;
        UpdateCalls++;
    }

    public void Delete(Match match)
    {
        _matches.Remove(match.MatchId);
        DeleteCalls++;
    }

    public Match? GetById(int matchId)
    {
        Match? match;
        bool found = _matches.TryGetValue(matchId, out match);
        if (!found)
        {
            return null;
        }

        return match;
    }

    public List<Match> GetByGroup(string groupName)
    {
        List<Match> result = new List<Match>();

        foreach (Match item in _matches.Values)
        {
            if (BelongsToGroup(item, groupName))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Match> GetByJourney(int journeyNumber)
    {
        List<Match> result = new List<Match>();

        foreach (Match item in _matches.Values)
        {
            if (BelongsToJourney(item, journeyNumber))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Match> GetByDateRange(DateTime from, DateTime to)
    {
        List<Match> result = new List<Match>();

        foreach (Match item in _matches.Values)
        {
            if (IsInDateRange(item, from, to))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Match> GetByStadium(int stadiumId)
    {
        List<Match> result = new List<Match>();

        foreach (Match item in _matches.Values)
        {
            if (item.Stadium != null && item.Stadium.Id == stadiumId)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Match> GetByPhase(Phase phase)
    {
        List<Match> result = new List<Match>();

        foreach (Match item in _matches.Values)
        {
            MatchElimination? eliminationMatch = item as MatchElimination;
            if (eliminationMatch != null && eliminationMatch.Phase == phase)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Match> GetAll()
    {
        return _matches.Values.ToList();
    }

    private static bool BelongsToGroup(Match match, string groupName)
    {
        MatchGroup? groupMatch = match as MatchGroup;
        if (groupMatch == null)
        {
            return false;
        }

        if (groupMatch.Group == null)
        {
            return false;
        }

        return groupMatch.Group.Name == groupName;
    }

    private static bool BelongsToJourney(Match match, int journeyNumber)
    {
        MatchGroup? groupMatch = match as MatchGroup;
        if (groupMatch == null)
        {
            return false;
        }

        if (groupMatch.Journey == null)
        {
            return false;
        }

        return groupMatch.Journey.Number == journeyNumber;
    }

    private static bool IsInDateRange(Match match, DateTime from, DateTime to)
    {
        return match.MatchDate >= from && match.MatchDate <= to;
    }
}
