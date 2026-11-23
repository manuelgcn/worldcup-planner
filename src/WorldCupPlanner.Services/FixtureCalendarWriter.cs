using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services;

// Escribe el fixture como iCalendar (RFC 5545): un VEVENT por partido.
public static class FixtureCalendarWriter
{
    public static string Write(IEnumerable<FixtureExportRow> rows, DateTime stampUtc)
    {
        return string.Empty;
    }
}
