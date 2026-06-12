using ProjectCopa;

namespace DataAccess.Interfaces;

// Criterio de busqueda que el repositorio traduce a una unica consulta sobre la
// jerarquia TPH "Matches". Todos los filtros son opcionales y se combinan (AND).
// From es inclusivo; To es EXCLUSIVO (el caso de uso lo calcula como inicio del dia
// siguiente para que "hasta" incluya el dia completo).
public sealed class MatchQueryCriteria
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? StadiumId { get; set; }
    public string? GroupName { get; set; }
    public Phase? Phase { get; set; }
}
