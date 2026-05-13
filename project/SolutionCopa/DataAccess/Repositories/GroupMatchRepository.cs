using DataAccess.Interfaces;
using ProjectCopa;

namespace DataAccess.Repositories;

public class GroupMatchRepository : IGroupMatchRepository
{
    private readonly List<Match> _storage = new();

    public Match? GetById(int id) => _storage.FirstOrDefault(m => m.MatchId == id);

    public IReadOnlyCollection<Match> GetByGroup(string groupName)
    {
        return _storage
            .OfType<MatchGroup>()
            .Where(mg => mg.Group != null && mg.Group.Name == groupName)
            .Cast<Match>()
            .ToList();
    }

    public IReadOnlyCollection<Match> GetAll() => _storage.AsReadOnly();

    public void Add(Match match)
    {
        if (!_storage.Any(m => m.MatchId == match.MatchId))
            _storage.Add(match);
    }

    public void Update(Match match)
    {
        Match? existing = GetById(match.MatchId);
        if (existing != null)
        {
            _storage.Remove(existing);
            _storage.Add(match);
        }
    }

    public void Delete(Match match)
    {
        _storage.RemoveAll(m => m.MatchId == match.MatchId);
    }

    public bool ExistsById(int id) => _storage.Any(m => m.MatchId == id);
}