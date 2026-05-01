using ProjectCopa.Interfaces;
using System;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

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
