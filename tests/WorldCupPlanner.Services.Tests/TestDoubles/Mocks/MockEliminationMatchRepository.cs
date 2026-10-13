using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

// Doble en memoria de IEliminationMatchRepository para los tests de servicios (Fase2, simulacion,
// knockout). Reproduce el comportamiento del antiguo repositorio basado en List<Match>: guarda las
// instancias tal cual (con sus navegaciones intactas), de modo que los servicios pueden leer
// HomeTeam/AwayTeam/MatchHome/MatchAway sin depender de una BD ni de prerrequisitos sembrados.
public sealed class MockEliminationMatchRepository : IEliminationMatchRepository
{
    private readonly List<Match> _storage = new();

    public Match? GetById(int id) => _storage.FirstOrDefault(match => match.MatchId == id);

    public IReadOnlyCollection<Match> GetByPhase(Phase phase) =>
        _storage.OfType<MatchElimination>().Where(match => match.Phase == phase).Cast<Match>().ToList();

    public IReadOnlyCollection<Match> GetAll() => _storage.AsReadOnly();

    public void Add(Match match)
    {
        if (!_storage.Any(stored => stored.MatchId == match.MatchId))
        {
            _storage.Add(match);
        }
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

    public void Delete(Match match) => _storage.RemoveAll(stored => stored.MatchId == match.MatchId);

    public bool ExistsById(int id) => _storage.Any(match => match.MatchId == id);

    public void Clear() => _storage.Clear();
}
