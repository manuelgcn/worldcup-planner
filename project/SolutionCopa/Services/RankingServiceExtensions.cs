using ProjectCopa;
using Services.Interfaces;

namespace Services;

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
}
