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
}
