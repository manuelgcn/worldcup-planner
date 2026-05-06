using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class DummyChampConf : IChampConf
{
    private readonly DateTime _startDate;
    public int StartDateCalls { get; private set; }

    public DummyChampConf(DateTime startDate)
    {
        _startDate = startDate;
    }

    public DateTime StartDate()
    {
        StartDateCalls++;
        return _startDate;
    }
}
