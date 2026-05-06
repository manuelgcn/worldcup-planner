using System.Collections.Concurrent;
using DataAccess.Interfaces;
using ProjectCopa;

namespace ServicesTest.TestDoubles;

/// <summary>
/// Single lookup repository that implements several read-only repository interfaces used in tests.
/// It stores objects by type and id and returns them via the corresponding interface methods.
/// </summary>
public sealed class MultiFakeLookupRepository : ITeamRepository, IStadiumRepository, IGroupRepository, IJourneyRepository, IChampConfRepository
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, object>> _store = new();

    public void Set<T>(int id, T item)
        where T : class
    {
        var type = typeof(T);
        ConcurrentDictionary<int, object> dict = _store.GetOrAdd(type, _ => new ConcurrentDictionary<int, object>());
        dict[id] = item!;
    }

    public T? GetByIdTyped<T>(int id)
        where T : class
    {
        var type = typeof(T);
        if (!_store.TryGetValue(type, out ConcurrentDictionary<int, object>? dict)) return null;
        if (!dict.TryGetValue(id, out object? obj)) return null;
        return obj as T;
    }

    // explicit interface implementations
    Team? ITeamRepository.GetById(int teamId) => GetByIdTyped<Team>(teamId);
    Stadium? IStadiumRepository.GetById(int stadiumId) => GetByIdTyped<Stadium>(stadiumId);
    Group? IGroupRepository.GetById(int groupId) => GetByIdTyped<Group>(groupId);
    Journey? IJourneyRepository.GetById(int journeyId) => GetByIdTyped<Journey>(journeyId);
    ChampConf? IChampConfRepository.GetById(int championshipConfigurationId) => GetByIdTyped<ChampConf>(championshipConfigurationId);
}

