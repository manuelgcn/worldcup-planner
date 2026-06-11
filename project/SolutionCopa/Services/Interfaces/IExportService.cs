using ProjectCopa;
using Services.DTOs;

namespace Services.Interfaces;

public interface IExportService
{
    ExportFileDto ExportFixture(DateRange range, ExportFormat format, User requestedBy);
    ExportFileDto ExportAuditLog(DateRange range, ExportFormat format, User requestedBy, AuditLogFilterDto? filter = null);
}
