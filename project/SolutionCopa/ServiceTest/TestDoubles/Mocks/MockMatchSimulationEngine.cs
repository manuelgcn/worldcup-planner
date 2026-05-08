using ProjectCopa;
using Services.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockMatchSimulationEngine : IMatchSimulationEngine
{
    public bool SimulateWasCalled { get; private set; }
    public int? LastSeedReceived { get; private set; }
    public Match? LastMatchReceived { get; private set; }

    public void Simulate(Match match, int simulationSeed)
    {
        SimulateWasCalled = true;
        LastSeedReceived = simulationSeed;
        LastMatchReceived = match;

        match.SetGoals(2, 1);
    }
}
