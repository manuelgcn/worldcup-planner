using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Interfaces;

namespace Services;

// Lee la configuracion de simulacion desde TournamentSettings y la proyecta en un DTO plano
// para la UI. Mantiene el repositorio de settings fuera de las paginas.
public class SimulationConfigurationService(ITournamentSettingsRepository settingsRepository)
    : ISimulationConfigurationService
{
    public SimulationConfigurationDto? GetConfiguration()
    {
        TournamentSettings? settings = settingsRepository.GetAll().FirstOrDefault();
        if (settings is null)
        {
            return null;
        }

        return new SimulationConfigurationDto
        {
            SelectedSimulationEngine = settings.SelectedSimulationEngine,
            SimulationSeed = settings.SimulationSeed
        };
    }
}
