using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IExportService
{
    ExportFileDto ExportFixtureIcs(DateRange range, User requestedBy);
    ExportFileDto ExportFixture(DateRange range, ExportFormat format, User requestedBy);
    ExportFileDto ExportAuditLog(DateRange range, ExportFormat format, User requestedBy, AuditLogFilterDto? filter = null);
}
