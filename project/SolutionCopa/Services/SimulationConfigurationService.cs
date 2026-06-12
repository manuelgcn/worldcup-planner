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
        throw new NotImplementedException();
    }
}
