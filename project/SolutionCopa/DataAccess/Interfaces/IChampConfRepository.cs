using ProjectCopa.Interfaces;

namespace DataAccess.Interfaces;

public interface IChampConfRepository
{
    IChampConf? GetById(int championshipConfigurationId);
}
