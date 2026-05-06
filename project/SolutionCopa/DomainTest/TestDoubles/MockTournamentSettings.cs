using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockTournamentSettings : ITournamentSettings
{
    private readonly DateTime _startDate;
    public int StartDateCalls { get; private set; }

    public MockTournamentSettings(DateTime startDate)
    {
        _startDate = startDate;
    }

    public DateTime StartDate()
    {
        StartDateCalls++;
        return _startDate;
    }
}
