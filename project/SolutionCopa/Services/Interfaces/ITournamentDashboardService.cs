using Services.DTOs;

namespace Services.Interfaces;

// Caso de uso de lectura del estado general del torneo para la home: reune los conteos,
// los flags de fase y el campeon en una sola proyeccion plana, consultando una unica vez
// la jerarquia TPH para no duplicar partidos eliminatorios.
public interface ITournamentDashboardService
{
    TournamentDashboardDto GetOverview();
}
