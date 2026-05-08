using ProjectCopa;

namespace Services.Interfaces;

public interface IMatchSimulationEngine
{
    void Simulate(Match match, int simulationSeed);
}
