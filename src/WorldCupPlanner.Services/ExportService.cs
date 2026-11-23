using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

public sealed class ExportService(
    IMatchRepository matchRepository,
    IAuditLogRepository auditLogRepository,
    IExporter exporter,
    IAuditLogService auditLogService) : IExportService
{
    // Tope de filas del export de auditoría; público para que los tests no repitan el número.
    public const int MaxAuditLogRows = 10_000;

    private const string InvalidDateRangeMessage = "'desde' debe ser menor o igual que 'hasta'.";
    private const string UnauthorizedMessage = "Solo los usuarios Admin y Periodista pueden exportar datos.";
    private const string TooManyAuditLogRowsMessage = "El registro de auditoría tiene más de {0} filas en ese rango. Elegí un rango de fechas más corto.";
    private const string NoMatchesInRangeMessage = "No hay partidos en ese rango de fechas. Probá con otras fechas.";
    private const string FixtureWorksheetName = "Fixture";
    private const string IcsContentType = "text/calendar; charset=utf-8";
    private const string AuditLogWorksheetName = "AuditLog";
    private const string CsvContentType = "text/csv; charset=utf-8";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ExportFileDto ExportFixture(DateRange range, ExportFormat format, User requestedBy)
    {
        EnsureAuthorized(requestedBy);
        (DateTime from, DateTime to) = NormalizeRange(range);

        List<FixtureExportRow> rows = matchRepository.GetByDateRange(from, to)
            .OrderBy(match => match.MatchDate)
            .ThenBy(match => match.MatchId)
            .Select(MapFixtureRow)
            .ToList();

        string fileName = BuildFileName("fixture", range, format);
        ExportFileDto file = BuildFile(rows, format, FixtureWorksheetName, fileName);
        LogExport(GetFixtureAuditAction(format), fileName, requestedBy);
        return file;
    }

    public ExportFileDto ExportFixtureIcs(DateRange range, User requestedBy)
    {
        EnsureAuthorized(requestedBy);
        (DateTime from, DateTime to) = NormalizeRange(range);

        List<FixtureExportRow> rows = matchRepository.GetByDateRange(from, to)
            .OrderBy(match => match.MatchDate)
            .ThenBy(match => match.MatchId)
            .Select(MapFixtureRow)
            .ToList();

        // RFC 5545 exige al menos un componente dentro de VCALENDAR: un calendario sin eventos no es válido.
        if (rows.Count == 0)
        {
            throw new ExportServiceException(NoMatchesInRangeMessage);
        }

        // DTSTAMP es metadato del archivo (cuándo se generó), igual que la marca de tiempo de la auditoría.
        string calendar = FixtureCalendarWriter.Write(rows, TimeProvider.System.GetUtcNow().UtcDateTime);
        string fileName = $"fixture_{range.From:yyyyMMdd}_{range.To:yyyyMMdd}.ics";
        ExportFileDto file = new ExportFileDto
        {
            // GetBytes no antepone BOM (el CSV sí lo lleva): un .ics tiene que empezar con BEGIN:VCALENDAR.
            Content = System.Text.Encoding.UTF8.GetBytes(calendar),
            ContentType = IcsContentType,
            FileName = fileName
        };
        LogExport(AuditActions.ExportFixtureIcs, fileName, requestedBy);
        return file;
    }

    public ExportFileDto ExportAuditLog(DateRange range, ExportFormat format, User requestedBy, AuditLogFilterDto? filter = null)
    {
        EnsureAuthorized(requestedBy);
        (DateTime from, DateTime to) = NormalizeRange(range);

        // Los límites exactos del filtro (día local de la pantalla, en UTC) ganan sobre los días del rango.
        // Se pide una fila de más solo para saber si el rango supera el tope: así SQL nunca devuelve el registro entero.
        List<AuditLog> logs = auditLogRepository.GetFiltered(
            filter?.From ?? from, filter?.To ?? to, filter?.Action, filter?.PerformedByEmail, MaxAuditLogRows + 1);

        if (logs.Count > MaxAuditLogRows)
        {
            throw new ExportServiceException(string.Format(TooManyAuditLogRowsMessage, MaxAuditLogRows));
        }

        // Con tope el repositorio devuelve de la más nueva a la más vieja; el archivo va de la más vieja a la más nueva.
        logs.Reverse();
        List<AuditLogExportRow> rows = logs.Select(MapAuditLogRow).ToList();

        // Sin "desde" la pantalla manda DateTime.MinValue: el nombre arranca en la fila más vieja exportada, no en 00010101.
        // Esa fila está en UTC y "hasta" es un día local: si cae después, el nombre arranca en "hasta" para no quedar al revés.
        DateRange namedRange = range.From.Date == DateTime.MinValue
            ? range with { From = rows.Count > 0 && rows[0].Timestamp.Date <= range.To.Date ? rows[0].Timestamp : range.To }
            : range;

        string fileName = BuildFileName("audit-log", namedRange, format);
        ExportFileDto file = BuildFile(rows, format, AuditLogWorksheetName, fileName);
        LogExport(GetAuditLogAuditAction(format), fileName, requestedBy);
        return file;
    }

    private ExportFileDto BuildFile<T>(IEnumerable<T> rows, ExportFormat format, string worksheetName, string fileName)
    {
        return new ExportFileDto
        {
            Content = exporter.Export(rows, format, worksheetName),
            ContentType = GetContentType(format),
            FileName = fileName
        };
    }

    private void LogExport(string action, string fileName, User requestedBy)
    {
        auditLogService.LogPerformedBy(
            action,
            $"Exported file {fileName}.",
            requestedBy.email,
            affectedEntityType: "Export");
    }

    private static void EnsureAuthorized(User? requestedBy)
    {
        if (requestedBy is null ||
            (!requestedBy.HasRole(UserRoles.Admin) && !requestedBy.HasRole(UserRoles.Periodista)))
        {
            throw new ExportServiceException(UnauthorizedMessage);
        }
    }

    private static (DateTime From, DateTime To) NormalizeRange(DateRange range)
    {
        ArgumentNullException.ThrowIfNull(range);
        if (range.From.Date > range.To.Date)
        {
            throw new ExportServiceException(InvalidDateRangeMessage);
        }

        return (range.From.Date, range.To.Date.AddDays(1).AddTicks(-1));
    }

    private static FixtureExportRow MapFixtureRow(Match match)
    {
        MatchGroup? groupMatch = match as MatchGroup;
        MatchElimination? eliminationMatch = match as MatchElimination;

        return new FixtureExportRow
        {
            MatchId = match.MatchId,
            MatchDate = match.MatchDate,
            Stage = groupMatch is not null ? "Group" : eliminationMatch?.Phase.ToString() ?? match.GetType().Name,
            Group = groupMatch?.Group?.Name,
            Journey = groupMatch?.Journey?.Number,
            MatchCode = groupMatch?.MatchCode ?? eliminationMatch?.MatchCode,
            HomeTeam = match.HomeTeam?.Name,
            AwayTeam = match.AwayTeam?.Name,
            Stadium = match.Stadium?.Name,
            HomeGoals = match.HomeGoals,
            AwayGoals = match.AwayGoals,
            WasSimulated = match.WasSimulated,
            HomePenaltyGoals = eliminationMatch?.HomePenaltyGoals,
            AwayPenaltyGoals = eliminationMatch?.AwayPenaltyGoals
        };
    }

    private static AuditLogExportRow MapAuditLogRow(AuditLog log)
    {
        return new AuditLogExportRow
        {
            Timestamp = log.Timestamp,
            Action = log.Action,
            Details = log.Details,
            TournamentSettingsId = log.TournamentSettingsId,
            PerformedByEmail = log.PerformedByEmail,
            EntityType = log.AffectedEntityType,
            EntityId = log.AffectedEntityId,
            PerformedByUserId = log.PerformedByUserId
        };
    }

    private static string BuildFileName(string prefix, DateRange range, ExportFormat format)
    {
        return $"{prefix}_{range.From:yyyyMMdd}_{range.To:yyyyMMdd}.{GetExtension(format)}";
    }

    private static string GetExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => "csv",
            ExportFormat.Xlsx => "xlsx",
            _ => throw new ExportServiceException("Formato de exportacion no soportado.")
        };
    }

    private static string GetContentType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => CsvContentType,
            ExportFormat.Xlsx => XlsxContentType,
            _ => throw new ExportServiceException("Formato de exportacion no soportado.")
        };
    }

    private static string GetFixtureAuditAction(ExportFormat format)
    {
        return format == ExportFormat.Csv ? AuditActions.ExportFixtureCsv : AuditActions.ExportFixtureXlsx;
    }

    private static string GetAuditLogAuditAction(ExportFormat format)
    {
        return format == ExportFormat.Csv ? AuditActions.ExportAuditCsv : AuditActions.ExportAuditXlsx;
    }
}
