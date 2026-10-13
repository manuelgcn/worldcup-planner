using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services;

// Orquestacion comun de la actualizacion de ranking post-partido: deriva la fase del tipo
// de partido y respeta el flag persistido RankingApplied para aplicar el Elo una sola vez
// (p. ej. simular y luego publicar el mismo partido). Debe invocarse ANTES del Update del
// repositorio de partidos para que el flag viaje en el mismo guardado.
public static class RankingServiceExtensions
{
    public static void ApplyMatchResultOnce(this IRankingService rankingService, Match match)
    {
        if (match.RankingApplied)
        {
            return;
        }

        MatchPhase phase = match is MatchGroup ? MatchPhase.GroupStage : MatchPhase.Knockout;
        rankingService.ApplyMatchResult(match, phase);
        match.MarkRankingApplied();
    }

    // Edicion de resultado (H7): si el partido ya tenia ranking aplicado, primero revierte el
    // efecto anterior y luego aplica el nuevo marcador. Asi el ranking queda como si el nuevo
    // resultado hubiera sido el unico aplicado (re-registrar el mismo marcador es idempotente).
    public static void ReapplyMatchResult(this IRankingService rankingService, Match match)
    {
        if (match.RankingApplied)
        {
            rankingService.RevertMatchResult(match);
        }

        rankingService.ApplyMatchResultOnce(match);
    }
}
