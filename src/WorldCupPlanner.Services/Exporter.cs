using System.Globalization;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class Exporter : IExporter
{
    private const string UnsupportedFormatMessage = "Formato de exportacion no soportado.";

    public byte[] Export<T>(IEnumerable<T> rows, ExportFormat format, string worksheetName)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return format switch
        {
            ExportFormat.Csv => ExportCsv(rows),
            ExportFormat.Xlsx => ExportXlsx(rows, worksheetName),
            _ => throw new ArgumentOutOfRangeException(nameof(format), UnsupportedFormatMessage)
        };
    }

    private static byte[] ExportCsv<T>(IEnumerable<T> rows)
    {
        using MemoryStream stream = new MemoryStream();
        using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
        CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };
        using CsvWriter csv = new CsvWriter(writer, configuration);
        TypeConverterOptions dateOptions = new TypeConverterOptions { Formats = ["O"] };
        csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(dateOptions);
        csv.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(dateOptions);

        csv.WriteHeader<T>();
        csv.NextRecord();
        csv.WriteRecords(rows);
        writer.Flush();

        return stream.ToArray();
    }

    private static byte[] ExportXlsx<T>(IEnumerable<T> rows, string worksheetName)
    {
        using XLWorkbook workbook = new XLWorkbook();
        IXLWorksheet worksheet = workbook.Worksheets.Add(worksheetName);
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        for (int column = 0; column < properties.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = properties[column].Name;
        }

        int rowNumber = 2;
        foreach (T row in rows)
        {
            for (int column = 0; column < properties.Length; column++)
            {
                SetCellValue(worksheet.Cell(rowNumber, column + 1), properties[column].GetValue(row));
            }

            rowNumber++;
        }

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.SheetView.FreezeRows(1);
        worksheet.ColumnsUsed().AdjustToContents();

        using MemoryStream stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case DateTime date:
                cell.Value = date;
                cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                break;
            case int integer:
                cell.Value = integer;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            default:
                cell.Value = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                break;
        }
    }
}
