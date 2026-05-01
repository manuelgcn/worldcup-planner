using System.Collections.Concurrent;
using ServicesTest.TestDoubles.Fakes_Stubs;
using DataAccess.Interfaces;

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
        var dict = _store.GetOrAdd(type, _ => new ConcurrentDictionary<int, object>());
        dict[id] = item!;
    }

    public T? GetByIdTyped<T>(int id)
        where T : class
    {
        var type = typeof(T);
        if (!_store.TryGetValue(type, out var dict)) return null;
        if (!dict.TryGetValue(id, out var obj)) return null;
        return obj as T;
    }

    // explicit interface implementations
    ProjectCopa.Interfaces.ITeam? ITeamRepository.GetById(int teamId) => GetByIdTyped<ProjectCopa.Interfaces.ITeam>(teamId);
    ProjectCopa.Interfaces.IStadium? IStadiumRepository.GetById(int stadiumId) => GetByIdTyped<ProjectCopa.Interfaces.IStadium>(stadiumId);
    ProjectCopa.Interfaces.IGroup? IGroupRepository.GetById(int groupId) => GetByIdTyped<ProjectCopa.Interfaces.IGroup>(groupId);
    ProjectCopa.Interfaces.IJourney? IJourneyRepository.GetById(int journeyId) => GetByIdTyped<ProjectCopa.Interfaces.IJourney>(journeyId);
    ProjectCopa.Interfaces.IChampConf? IChampConfRepository.GetById(int championshipConfigurationId) => GetByIdTyped<ProjectCopa.Interfaces.IChampConf>(championshipConfigurationId);
}

