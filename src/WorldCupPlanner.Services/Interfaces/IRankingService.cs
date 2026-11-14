using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Interfaces;

public interface IRankingService
{
    void ApplyMatchResult(Match match, MatchPhase phase);

    // Revierte el efecto Elo que este partido aplico (usando los deltas guardados) y limpia su
    // marca de aplicado, para permitir reaplicar un resultado editado (H7). No persiste los equipos:
    // el que llama los guarda una vez terminado (el valor intermedio puede quedar fuera de rango).
    void RevertMatchResult(Match match);
}