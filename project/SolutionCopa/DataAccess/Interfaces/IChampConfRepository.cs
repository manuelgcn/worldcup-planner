using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IChampConfRepository
{
    ChampConf? GetById(int championshipConfigurationId);
}
