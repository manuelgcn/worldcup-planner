using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class StubChampConf : IChampConf
{
    private readonly DateTime _startDate;

    public StubChampConf(DateTime startDate)
    {
        _startDate = startDate;
    }

    public DateTime StartDate()
    {
        return _startDate;
    }
}
