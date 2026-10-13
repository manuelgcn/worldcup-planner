using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

// Caso de uso de lectura de la configuracion de simulacion: expone a la UI el motor y la
// semilla configurados sin que la pagina conozca el repositorio de TournamentSettings.
// Devuelve null cuando el torneo aun no tiene configuracion cargada.
public interface ISimulationConfigurationService
{
    SimulationConfigurationDto? GetConfiguration();
}
