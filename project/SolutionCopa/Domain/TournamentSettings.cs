namespace ProjectCopa;

public class TournamentSettings
{
    private const int DefaultStartDateYear = 2026;
    private const int DefaultStartDateMonth = 6;
    private const int DefaultStartDateDay = 1;
    private const int DefaultMaxMatchesPerDay = 3;
    private const int DefaultDaysBetweenJourneys = 3;
    private const int DefaultFixtureSeed = 0;
    private const int DefaultCrossingsSeed = 0;
    private const int DefaultSimulationSeed = 0;
    private const int DefaultCompletionSeed = 0;

    public DateTime StartDate { get; } = new DateTime(DefaultStartDateYear, DefaultStartDateMonth, DefaultStartDateDay);
    public int MaxMatchesPerDay { get; } = DefaultMaxMatchesPerDay;
    public int DaysBetweenJourneys { get; } = DefaultDaysBetweenJourneys;
    public int FixtureSeed { get; } = DefaultFixtureSeed;
    public int CrossingsSeed { get; } = DefaultCrossingsSeed;
    public int SimulationSeed { get; } = DefaultSimulationSeed;
    public int CompletionSeed { get; } = DefaultCompletionSeed;
}
