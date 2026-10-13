namespace WorldCupPlanner.Services.DTOs;

// Read model plano de la configuracion de simulacion para la UI: el motor seleccionado
// y la semilla determinista persistidos en TournamentSettings, sin exponer el repositorio.
public sealed class SimulationConfigurationDto
{
    public string SelectedSimulationEngine { get; set; } = string.Empty;
    public int SimulationSeed { get; set; }
}
