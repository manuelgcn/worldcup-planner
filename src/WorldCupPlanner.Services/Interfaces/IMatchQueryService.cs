using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

// Caso de uso de consulta de partidos: combina los filtros y los resuelve en una sola
// consulta al repositorio, devolviendo proyecciones planas listas para la UI.
public interface IMatchQueryService
{
    IReadOnlyCollection<MatchListItemDto> Search(MatchFilterDto filter);
}
