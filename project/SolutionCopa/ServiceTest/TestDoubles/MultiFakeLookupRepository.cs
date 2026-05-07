using System.Collections.Concurrent;
using DataAccess.Interfaces;
using ProjectCopa;

namespace ServiceTest.TestDoubles;

/// <summary>
/// Single lookup repository that implements several read-only repository interfaces used in tests.
/// It stores objects by type and id and returns them via the corresponding interface methods.
/// </summary>
public sealed class MultiFakeLookupRepository : ITeamRepository, IStadiumRepository, IGroupRepository, IJourneyRepository, ITournamentSettingsRepository
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, object>> _store = new();

    public void Set<T>(int id, T item)
        where T : class
    {
        Type type = typeof(T);
        ConcurrentDictionary<int, object> dict = _store.GetOrAdd(type, _ => new ConcurrentDictionary<int, object>());
        dict[id] = item!;
    }

    public T? GetByIdTyped<T>(int id)
        where T : class
    {
        Type type = typeof(T);
        if (!_store.TryGetValue(type, out ConcurrentDictionary<int, object>? dict)) return null;
        if (!dict.TryGetValue(id, out object? obj)) return null;
        T? result = obj as T;
        return result;
    }

    // explicit interface implementations
    Team? ITeamRepository.GetById(int teamId) => GetByIdTyped<Team>(teamId);
    Stadium? IStadiumRepository.GetById(int stadiumId) => GetByIdTyped<Stadium>(stadiumId);
    Group? IGroupRepository.GetById(int groupId) => GetByIdTyped<Group>(groupId);
    Journey? IJourneyRepository.GetById(int journeyId) => GetByIdTyped<Journey>(journeyId);
    TournamentSettings? ITournamentSettingsRepository.GetById(int tournamentSettingsId) => GetByIdTyped<TournamentSettings>(tournamentSettingsId);
}

