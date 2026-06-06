namespace Services.Interfaces;

public interface ITournamentSimulationService
{
    void SimulateGroupStage(int seed, string? performedByEmail);
    void SimulateGroupStageAndGenerateKnockout(int seed);
}
