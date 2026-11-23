using System.Text;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Tests;

[TestClass]
public sealed class FixtureCalendarWriterTests
{
    private const string CrLf = "\r\n";
    private static readonly DateTime Stamp = new DateTime(2026, 6, 1, 9, 30, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Write_WithoutMatches_ReturnsAnEmptyCalendarWithVersionAndProductId()
    {
        string calendar = FixtureCalendarWriter.Write(Array.Empty<FixtureExportRow>(), Stamp);

        Assert.AreEqual(
            "BEGIN:VCALENDAR" + CrLf +
            "VERSION:2.0" + CrLf +
            "PRODID:-//Golazo//WorldCupPlanner//ES" + CrLf +
            "END:VCALENDAR" + CrLf,
            calendar);
    }

    [TestMethod]
    public void Write_GroupMatch_WritesOneEventWithStableUidTwoHourEndStadiumAndGroup()
    {
        string calendar = FixtureCalendarWriter.Write(new[] { GroupRow() }, Stamp);

        StringAssert.Contains(
            calendar,
            "BEGIN:VEVENT" + CrLf +
            "UID:match-12@worldcupplanner" + CrLf +
            "DTSTAMP:20260601T093000Z" + CrLf +
            "DTSTART:20260611T140000" + CrLf +
            "DTEND:20260611T160000" + CrLf +
            "SUMMARY:Uruguay vs Argentina" + CrLf +
            "LOCATION:Centenario" + CrLf +
            "DESCRIPTION:Grupo A" + CrLf +
            "END:VEVENT" + CrLf);
    }

    // La app guarda y muestra la hora del partido sin zona horaria: el evento usa hora flotante
    // (sin Z ni TZID) y no convierte, aunque la fecha llegue marcada como UTC.
    [TestMethod]
    public void Write_KeepsTheMatchTimeAsStoredWhateverItsKind()
    {
        FixtureExportRow row = GroupRow(matchDate: new DateTime(2026, 6, 11, 23, 0, 0, DateTimeKind.Utc));

        string calendar = FixtureCalendarWriter.Write(new[] { row }, Stamp);

        StringAssert.Contains(calendar, "DTSTART:20260611T230000" + CrLf + "DTEND:20260612T010000" + CrLf);
    }

    [TestMethod]
    public void Write_EliminationMatch_DescribesThePhaseInSpanish()
    {
        FixtureExportRow row = new FixtureExportRow
        {
            MatchId = 73,
            MatchDate = new DateTime(2026, 6, 28, 14, 0, 0),
            Stage = "RoundOf32",
            HomeTeam = "Uruguay",
            AwayTeam = "Japón",
            Stadium = "Centenario"
        };

        string calendar = FixtureCalendarWriter.Write(new[] { row }, Stamp);

        StringAssert.Contains(calendar, "DESCRIPTION:16avos de final" + CrLf);
    }

    [TestMethod]
    public void Write_UndefinedTeamAndStadium_UsesThePlaceholderAndOmitsTheLocation()
    {
        FixtureExportRow row = new FixtureExportRow
        {
            MatchId = 90,
            MatchDate = new DateTime(2026, 7, 5, 14, 0, 0),
            Stage = "SemiFinal",
            HomeTeam = null,
            AwayTeam = "Argentina",
            Stadium = null
        };

        string calendar = FixtureCalendarWriter.Write(new[] { row }, Stamp);

        StringAssert.Contains(calendar, "SUMMARY:A definir vs Argentina" + CrLf);
        Assert.IsFalse(calendar.Contains("LOCATION"));
    }

    // Los nombres llegan de datos importados: un salto de línea no puede abrir otra propiedad ni otro evento.
    [TestMethod]
    public void Write_EscapesTextSoANameCannotInjectProperties()
    {
        FixtureExportRow row = GroupRow(stadium: "Uno; Dos, Tres\\Cuatro\r\nBEGIN:VEVENT\nEND:VCALENDAR");

        string calendar = FixtureCalendarWriter.Write(new[] { row }, Stamp);

        StringAssert.Contains(calendar, "LOCATION:Uno\\; Dos\\, Tres\\\\Cuatro\\nBEGIN:VEVENT\\nEND:VCALENDAR" + CrLf);
        Assert.AreEqual(1, PhysicalLines(calendar).Count(line => line == "BEGIN:VEVENT"));
        Assert.AreEqual(1, PhysicalLines(calendar).Count(line => line == "END:VCALENDAR"));
    }

    [TestMethod]
    public void Write_FoldsLongLinesAtSeventyFiveOctetsWithoutSplittingACharacter()
    {
        string stadium = string.Concat(Enumerable.Repeat("Estadio Olímpico Ñandú 🏟 ", 12));
        UTF8Encoding strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        string calendar = FixtureCalendarWriter.Write(new[] { GroupRow(stadium: stadium) }, Stamp);

        List<string> lines = PhysicalLines(calendar);
        Assert.IsTrue(lines.Count(line => line.StartsWith(' ')) >= 4, "the long value should span several continuation lines");
        foreach (string line in lines)
        {
            // GetByteCount con el codificador estricto lanza si un par sustituto quedó partido entre dos líneas.
            Assert.IsTrue(strictUtf8.GetByteCount(line) <= 75, $"'{line}' is longer than 75 octets");
        }

        string unfolded = calendar.Replace(CrLf + " ", string.Empty);
        StringAssert.Contains(unfolded, "LOCATION:" + stadium + CrLf);
    }

    [TestMethod]
    public void Write_EndsEveryLineWithCrLf()
    {
        string calendar = FixtureCalendarWriter.Write(new[] { GroupRow(), GroupRow(matchId: 13) }, Stamp);

        Assert.IsTrue(calendar.EndsWith(CrLf));
        string withoutLineBreaks = calendar.Replace(CrLf, string.Empty);
        Assert.IsFalse(withoutLineBreaks.Contains('\r') || withoutLineBreaks.Contains('\n'));
        Assert.AreEqual(2, PhysicalLines(calendar).Count(line => line == "BEGIN:VEVENT"));
    }

    private static List<string> PhysicalLines(string calendar) =>
        calendar.Split(CrLf, StringSplitOptions.RemoveEmptyEntries).ToList();

    private static FixtureExportRow GroupRow(int matchId = 12, DateTime? matchDate = null, string? stadium = "Centenario")
    {
        return new FixtureExportRow
        {
            MatchId = matchId,
            MatchDate = matchDate ?? new DateTime(2026, 6, 11, 14, 0, 0),
            Stage = "Group",
            Group = "A",
            HomeTeam = "Uruguay",
            AwayTeam = "Argentina",
            Stadium = stadium
        };
    }
}
