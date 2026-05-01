using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class FakeChampConfRepository : IChampConfRepository
{
    private readonly Dictionary<int, IChampConf> _configs = new();

    public void Set(int id, IChampConf config)
    {
        _configs[id] = config;
    }

    public IChampConf? GetById(int championshipConfigurationId)
    {
        IChampConf? config;
        var found = _configs.TryGetValue(championshipConfigurationId, out config);
        if (!found)
        {
            return null;
        }

        return config;
    }
}
