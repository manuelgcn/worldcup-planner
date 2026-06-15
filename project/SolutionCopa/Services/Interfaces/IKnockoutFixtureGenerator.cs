using ProjectCopa;
using ProjectCopa.Models;

namespace Services.Interfaces;

public interface IKnockoutFixtureGenerator
{
    List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams);

    bool IsBracketGenerated();
    void GenerateBracket(int? crossingsSeedOverride, string? performedByEmail);
    void SimulateNextRound(string? performedByEmail);
    void SimulateUntilFinal(string? performedByEmail);

    // Avance manual: crea la siguiente ronda con los ganadores de la fase actual
    // dejandola PENDIENTE (sin simular), para cargar los resultados a mano.
    bool CanAdvanceToNextRound();
    void AdvanceToNextRound(string? performedByEmail);
}
