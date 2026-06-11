using ClosedXML.Excel;
using Services;
using Services.DTOs;
using Services.Interfaces;

namespace ServiceTest;

[TestClass]
public sealed class ExporterTests
{
    private readonly IExporter _exporter = new Exporter();

    [TestMethod]
    public void ExportCsvWritesUtf8BomHeadersAndEscapedValues()
    {
        FixtureExportRow[] rows =
        [
            new FixtureExportRow
            {
                MatchId = 1,
                MatchDate = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc),
                Stage = "Group",
                HomeTeam = "Uruguay, Celeste",
                AwayTeam = "Argentina"
            }
        ];

        byte[] result = _exporter.Export(rows, ExportFormat.Csv, "Fixture");
        string csv = System.Text.Encoding.UTF8.GetString(result);

        Assert.AreEqual(0xEF, result[0]);
        Assert.IsTrue(csv.Contains("MatchId"));
        Assert.IsTrue(csv.Contains("\"Uruguay, Celeste\""));
        Assert.IsTrue(csv.Contains("2026-06-11T18:00:00.0000000Z"));
    }

    [TestMethod]
    public void ExportCsvWithNoRowsStillWritesHeaders()
    {
        byte[] result = _exporter.Export(Array.Empty<AuditLogExportRow>(), ExportFormat.Csv, "AuditLog");
        string csv = System.Text.Encoding.UTF8.GetString(result);

        Assert.IsTrue(csv.Contains("Timestamp"));
        Assert.IsTrue(csv.Contains("Action"));
    }

    [TestMethod]
    public void ExportXlsxWritesWorksheetHeadersAndTypedDate()
    {
        AuditLogExportRow[] rows =
        [
            new AuditLogExportRow
            {
                Timestamp = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc),
                Action = "CREATE_MATCH",
                Details = "Created"
            }
        ];

        byte[] result = _exporter.Export(rows, ExportFormat.Xlsx, "AuditLog");
        using MemoryStream stream = new MemoryStream(result);
        using XLWorkbook workbook = new XLWorkbook(stream);
        IXLWorksheet sheet = workbook.Worksheet("AuditLog");

        Assert.AreEqual("Timestamp", sheet.Cell(1, 1).GetString());
        Assert.AreEqual("CREATE_MATCH", sheet.Cell(2, 2).GetString());
        Assert.AreEqual(XLDataType.DateTime, sheet.Cell(2, 1).DataType);
    }
}
