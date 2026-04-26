namespace ProjectCopa;

public class TournamentSettings
{
    private const int DefaultStartDateYear = 2026;
    private const int DefaultStartDateMonth = 6;
    private const int DefaultStartDateDay = 1;
    private const int DefaultMaxMatchesPerDay = 3;
    private const int DefaultDaysBetweenJourneys = 3;

    public DateTime StartDate { get; } = new DateTime(DefaultStartDateYear, DefaultStartDateMonth, DefaultStartDateDay);
    public int MaxMatchesPerDay { get; } = DefaultMaxMatchesPerDay;
    public int DaysBetweenJourneys { get; } = DefaultDaysBetweenJourneys;
}
