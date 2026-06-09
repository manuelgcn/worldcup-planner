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
    private const int MaxAllowedMatchesPerDay = 3;
    private const string DefaultSimulationEngine = "Probabilístico";
    private const string InvalidMaxMatchesPerDayMessage = "MaxMatchesPerDay must be between 1 and 3 inclusive.";
    private const string InvalidDaysBetweenJourneysMessage = "DaysBetweenJourneys must be greater than or equal to 1.";
    private const string InvalidSimulationEngineMessage = "SelectedSimulationEngine cannot be null or whitespace.";

    public int Id { get; }
    public DateTime StartDate { get; }
    public int MaxMatchesPerDay { get; }
    public int DaysBetweenJourneys { get; }
    public int FixtureSeed { get; }
    public int CrossingsSeed { get; }
    public int SimulationSeed { get; }
    public int CompletionSeed { get; }
    public string SelectedSimulationEngine { get; private set; }

    public TournamentSettings()
        : this(
            DefaultId,
            CreateDefaultStartDate(),
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
        ValidateDaysBetweenJourneys(daysBetweenJourneys);
        Id = id;
        StartDate = startDate;
        MaxMatchesPerDay = maxMatchesPerDay;
        DaysBetweenJourneys = daysBetweenJourneys;
        FixtureSeed = fixtureSeed;
        CrossingsSeed = crossingsSeed;
        SimulationSeed = simulationSeed;
        CompletionSeed = completionSeed;
        SelectedSimulationEngine = DefaultSimulationEngine;
    }

    public void SelectSimulationEngine(string engineName)
    {
        ValidateSimulationEngineName(engineName);
        SelectedSimulationEngine = engineName;
    }

    private static void ValidateSimulationEngineName(string engineName)
    {
        if (string.IsNullOrWhiteSpace(engineName))
        {
            throw new SettingsException(InvalidSimulationEngineMessage);
        }
    }

    private static void ValidateMaxMatchesPerDay(int maxMatchesPerDay)
    {
        if (maxMatchesPerDay < 1 || maxMatchesPerDay > MaxAllowedMatchesPerDay)
        {
            throw new SettingsException(InvalidMaxMatchesPerDayMessage);
        }
    }

    private static void ValidateDaysBetweenJourneys(int daysBetweenJourneys)
    {
        if (daysBetweenJourneys < 1)
        {
            throw new SettingsException(InvalidDaysBetweenJourneysMessage);
        }
    }

    private static DateTime CreateDefaultStartDate()
    {
        return new DateTime(DefaultStartDateYear, DefaultStartDateMonth, DefaultStartDateDay);
    }
}
