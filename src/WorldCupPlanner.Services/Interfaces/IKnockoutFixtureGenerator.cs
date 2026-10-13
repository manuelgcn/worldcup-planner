using WorldCupPlanner.Domain;
using WorldCupPlanner.Domain.Models;

namespace WorldCupPlanner.Services.Interfaces;

public interface IKnockoutFixtureGenerator
{
    List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams);

    bool IsBracketGenerated();
    // Completo = final y tercer puesto creados y todos los partidos con resultado (simulado o a mano).
    bool IsBracketComplete();
    void GenerateBracket(int? crossingsSeedOverride, string? performedByEmail);
    void SimulateNextRound(string? performedByEmail);
    void SimulateUntilFinal(string? performedByEmail);

    // Avance manual: crea la siguiente ronda con los ganadores de la fase actual
    // dejandola PENDIENTE (sin simular), para cargar los resultados a mano.
    bool CanAdvanceToNextRound();
    void AdvanceToNextRound(string? performedByEmail);
}
