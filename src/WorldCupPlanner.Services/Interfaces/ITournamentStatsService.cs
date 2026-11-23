using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

// Caso de uso de lectura "ver estadisticas del torneo": resume en una sola proyeccion plana
// los partidos que ya tienen resultado.
public interface ITournamentStatsService
{
    TournamentStatsDto GetStats();
}
