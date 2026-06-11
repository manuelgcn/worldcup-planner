namespace Services.Interfaces;

public interface ISimulationEngineRegistry
{
    IReadOnlyCollection<string> AvailableEngineNames { get; }
    IMatchSimulationEngine Resolve(string engineName);
}
