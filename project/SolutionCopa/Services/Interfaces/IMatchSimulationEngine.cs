using ProjectCopa;

namespace Services.Interfaces;

public interface IMatchSimulationEngine
{
    string Name { get; }
    void Simulate(Match match, int simulationSeed);
}
