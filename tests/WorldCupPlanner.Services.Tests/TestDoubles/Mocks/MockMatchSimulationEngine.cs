using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public sealed class MockMatchSimulationEngine : IMatchSimulationEngine
{
    // Por defecto usa el nombre del motor por defecto de TournamentSettings, asi los tests
    // que resuelven el motor via SimulationEngineRegistry encuentran este mock sin setup extra.
    public string Name { get; set; } = ProbabilisticSimulationEngine.EngineName;

    public bool SimulateWasCalled { get; private set; }
    public int? LastSeedReceived { get; private set; }
    public Match? LastMatchReceived { get; private set; }
    public int SimulateCalls { get; private set; }

    public void Simulate(Match match, int simulationSeed)
    {
        SimulateWasCalled = true;
        SimulateCalls++;
        LastSeedReceived = simulationSeed;
        LastMatchReceived = match;

        match.SetGoals(2, 1);
    }
}
