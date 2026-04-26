using ProjectCopa.Exceptions;

namespace ProjectCopa;

public class TournamentSettings
{
    private const int DefaultId = 0;
    private const int DefaultStartDateYear = 2026;
    private const int DefaultStartDateMonth = 6;
    private const int DefaultStartDateDay = 1;
    private const int DefaultMaxMatchesPerDay = 3;
    private const int DefaultDaysBetweenJourneys = 3;
    private const int DefaultSeed = 0;
    private const string InvalidMaxMatchesPerDayMessage = "MaxMatchesPerDay must be greater than or equal to 1.";

    public int Id { get; }
    public DateTime StartDate { get; }
    public int MaxMatchesPerDay { get; }
    public int DaysBetweenJourneys { get; }
    public int FixtureSeed { get; }
    public int CrossingsSeed { get; }
    public int SimulationSeed { get; }
    public int CompletionSeed { get; }

    public TournamentSettings()
        : this(
            DefaultId,
            new DateTime(DefaultStartDateYear, DefaultStartDateMonth, DefaultStartDateDay),
            DefaultMaxMatchesPerDay,
            DefaultDaysBetweenJourneys,
            DefaultSeed,
            DefaultSeed,
            DefaultSeed,
            DefaultSeed)
    {
    }

    public TournamentSettings(
        int id,
        DateTime startDate,
        int maxMatchesPerDay,
        int daysBetweenJourneys,
        int fixtureSeed,
        int crossingsSeed,
        int simulationSeed,
        int completionSeed)
    {
        ValidateMaxMatchesPerDay(maxMatchesPerDay);
        Id = id;
        StartDate = startDate;
        MaxMatchesPerDay = maxMatchesPerDay;
        DaysBetweenJourneys = daysBetweenJourneys;
        FixtureSeed = fixtureSeed;
        CrossingsSeed = crossingsSeed;
        SimulationSeed = simulationSeed;
        CompletionSeed = completionSeed;
    }

    private static void ValidateMaxMatchesPerDay(int maxMatchesPerDay)
    {
        if (maxMatchesPerDay < 1)
        {
            throw new SettingsException(InvalidMaxMatchesPerDayMessage);
        }
    }
}
