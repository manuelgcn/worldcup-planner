using ProjectCopa;

namespace Services.DTOs;

// Filtros opcionales para la consulta de partidos. Todos son opcionales y se
// combinan (AND). El rango de fechas es por dia completo: From incluye desde el
// inicio de ese dia y To incluye todo el dia indicado (ver MatchQueryService).
public class MatchFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? StadiumId { get; set; }
    public string? GroupName { get; set; }
    public Phase? Phase { get; set; }
}
