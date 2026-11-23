using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

// Caso de uso de lectura "ver la ficha de un equipo": quien es, como le va en su grupo,
// que partidos juega y hasta donde llego.
public interface ITeamProfileService
{
    TeamProfileDto GetProfile(int teamId);
}
