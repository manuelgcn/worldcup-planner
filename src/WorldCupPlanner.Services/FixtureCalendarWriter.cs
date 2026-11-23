using System.Globalization;
using System.Text;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services;

// Escribe el fixture como iCalendar (RFC 5545): un VEVENT por partido.
public static class FixtureCalendarWriter
{
    private const string LineBreak = "\r\n";
    private const int MaxLineOctets = 75;
    private const int MatchDurationHours = 2;
    private const string DateTimeFormat = "yyyyMMdd'T'HHmmss";
    private const string ProductId = "-//Golazo//WorldCupPlanner//ES";
    private const string UidFormat = "match-{0}@worldcupplanner";
    private const string SummaryFormat = "{0} vs {1}";
    private const string UndecidedTeamName = "A definir";
    private const string GroupDescriptionFormat = "Grupo {0}";
    private const string RoundOf32Description = "16avos de final";
    private const string RoundOf16Description = "Octavos de final";
    private const string QuarterFinalsDescription = "Cuartos de final";
    private const string SemiFinalDescription = "Semifinal";
    private const string ThirdPlaceDescription = "Tercer puesto";
    private const string FinalDescription = "Final";

    public static string Write(IEnumerable<FixtureExportRow> rows, DateTime stampUtc)
    {
        ArgumentNullException.ThrowIfNull(rows);

        StringBuilder calendar = new StringBuilder();
        AppendLine(calendar, "BEGIN:VCALENDAR");
        AppendLine(calendar, "VERSION:2.0");
        AppendLine(calendar, "PRODID:" + ProductId);

        foreach (FixtureExportRow row in rows)
        {
            AppendLine(calendar, "BEGIN:VEVENT");
            AppendLine(calendar, "UID:" + string.Format(CultureInfo.InvariantCulture, UidFormat, row.MatchId));
            AppendLine(calendar, "DTSTAMP:" + Format(stampUtc) + "Z");
            // Hora flotante (sin Z ni TZID): la app guarda y muestra la hora del partido tal cual, sin zona horaria.
            AppendLine(calendar, "DTSTART:" + Format(row.MatchDate));
            AppendLine(calendar, "DTEND:" + Format(row.MatchDate.AddHours(MatchDurationHours)));
            AppendLine(calendar, "SUMMARY:" + Escape(string.Format(
                SummaryFormat, row.HomeTeam ?? UndecidedTeamName, row.AwayTeam ?? UndecidedTeamName)));
            if (row.Stadium is not null)
            {
                AppendLine(calendar, "LOCATION:" + Escape(row.Stadium));
            }

            AppendLine(calendar, "DESCRIPTION:" + Escape(Describe(row)));
            AppendLine(calendar, "END:VEVENT");
        }

        AppendLine(calendar, "END:VCALENDAR");
        return calendar.ToString();
    }

    private static string Format(DateTime value)
    {
        return value.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
    }

    private static string Describe(FixtureExportRow row)
    {
        if (row.Group is not null)
        {
            return string.Format(GroupDescriptionFormat, row.Group);
        }

        return row.Stage switch
        {
            nameof(Phase.RoundOf32) => RoundOf32Description,
            nameof(Phase.RoundOf16) => RoundOf16Description,
            nameof(Phase.QuarterFinals) => QuarterFinalsDescription,
            nameof(Phase.SemiFinal) => SemiFinalDescription,
            nameof(Phase.ThirdPlace) => ThirdPlaceDescription,
            nameof(Phase.Final) => FinalDescription,
            _ => row.Stage
        };
    }

    // RFC 5545 §3.3.11. La barra invertida va primero para no volver a escapar las que agregan los demás reemplazos.
    private static string Escape(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r", string.Empty)
            .Replace("\n", "\\n");
    }

    // RFC 5545 §3.1: ninguna línea pasa de 75 octetos sin contar el CRLF. La continuación empieza con un
    // espacio, que cuenta para su propia línea, y se corta entre caracteres completos, nunca en medio de uno.
    private static void AppendLine(StringBuilder calendar, string line)
    {
        int octets = 0;
        foreach (Rune rune in line.EnumerateRunes())
        {
            if (octets + rune.Utf8SequenceLength > MaxLineOctets)
            {
                calendar.Append(LineBreak).Append(' ');
                octets = 1;
            }

            calendar.Append(rune.ToString());
            octets += rune.Utf8SequenceLength;
        }

        calendar.Append(LineBreak);
    }
}
