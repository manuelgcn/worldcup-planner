using ProjectCopa.Interfaces;

namespace ProjectCopa;

public class TournamentSettings : ITournamentSettings
{
    private readonly DateTime _startDate;

    public TournamentSettings(DateTime startDate)
    {
        _startDate = startDate;
    }

    public DateTime StartDate()
    {
        return _startDate;
    }
}
