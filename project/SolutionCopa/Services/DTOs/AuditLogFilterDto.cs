namespace Services.DTOs;

// Filtros opcionales para consultar el log de auditoria; los de texto
// matchean parcial y case-insensitive, el rango de fechas es inclusivo.
public class AuditLogFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Action { get; set; }
    public string? PerformedByEmail { get; set; }
}
