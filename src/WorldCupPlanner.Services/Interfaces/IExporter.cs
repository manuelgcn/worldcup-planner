namespace WorldCupPlanner.Services.Interfaces;

public interface IExporter
{
    byte[] Export<T>(IEnumerable<T> rows, ExportFormat format, string worksheetName);
}
