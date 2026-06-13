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
    private const string FromMustBeLessOrEqualToMessage = "'from' must be less than or equal to 'to'";

    public IReadOnlyCollection<MatchListItemDto> Search(MatchFilterDto filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureDateRangeIsValid(filter);

        MatchQueryCriteria criteria = BuildCriteria(filter);
        return matchRepository.Search(criteria).Select(MapToListItem).ToList();
    }

    private static void EnsureDateRangeIsValid(MatchFilterDto filter)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From.Value > filter.To.Value)
        {
            throw new MatchServicesException(FromMustBeLessOrEqualToMessage);
        }
    }

    // "Desde" arranca al inicio del dia; "hasta" abarca el dia completo, por eso el limite
    // superior es el inicio del dia siguiente (exclusivo). Asi el repositorio no necesita
    // conocer la semantica de "dia completo": solo aplica >= From y < To.
    private static MatchQueryCriteria BuildCriteria(MatchFilterDto filter)
    {
        return new MatchQueryCriteria
        {
            From = filter.From?.Date,
            To = filter.To?.Date.AddDays(1),
            StadiumId = filter.StadiumId,
            GroupName = filter.GroupName,
            Phase = filter.Phase
        };
    }

    private static MatchListItemDto MapToListItem(Match match)
    {
        return new MatchListItemDto
        {
            MatchId = match.MatchId,
            MatchDate = match.MatchDate,
            HomeTeamName = match.HomeTeam?.Name,
            AwayTeamName = match.AwayTeam?.Name,
            HomeGoals = match.HomeGoals,
            AwayGoals = match.AwayGoals,
            StadiumName = match.Stadium?.Name,
            GroupName = (match as MatchGroup)?.Group?.Name,
            Phase = (match as MatchElimination)?.Phase,
            WasSimulated = match.WasSimulated,
            HasResult = match.HasResult
        };
    }
}
