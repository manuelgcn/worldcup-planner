using ProjectCopa;

namespace Services.Interfaces;

public interface IRankingService
{
    void ApplyMatchResult(Match match, MatchPhase phase);

    // Revierte el efecto Elo que este partido aplico (usando los deltas guardados) y limpia su
    // marca de aplicado, para permitir reaplicar un resultado editado (H7).
    void RevertMatchResult(Match match);
}