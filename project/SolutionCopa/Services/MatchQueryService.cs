using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

// Controlador GRASP del caso de uso "consultar partidos": valida el rango de fechas,
// traduce los filtros opcionales a un MatchQueryCriteria y delega en el repositorio una
// UNICA consulta sobre la jerarquia TPH (sin combinar repositorios, sin materializar todo
// en memoria). Mapea cada partido a una proyeccion plana lista para la UI.
public class MatchQueryService(IMatchRepository matchRepository) : IMatchQueryService
{
    public IReadOnlyCollection<MatchListItemDto> Search(MatchFilterDto filter)
    {
        throw new NotImplementedException();
    }
}
