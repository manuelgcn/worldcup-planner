using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Interfaces;

public interface IMatchSimulationEngine
{
    string Name { get; }
    void Simulate(Match match, int simulationSeed);
}
