namespace Services.Interfaces;

public interface ITournamentSimulationService
{
    void SimulateGroupStageAndGenerateKnockout(int seed);
}
