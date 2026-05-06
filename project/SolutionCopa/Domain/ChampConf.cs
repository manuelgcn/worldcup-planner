using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class ChampConf : IChampConf
{
    private readonly DateTime _startDate;

    public ChampConf(DateTime startDate)
    {
        _startDate = startDate;
    }

    public DateTime StartDate()
    {
        return _startDate;
    }
}
