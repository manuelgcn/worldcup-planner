using ProjectCopa;

namespace Services.Interfaces;

public interface IRankingService
{
    void ApplyMatchResult(Match match, MatchPhase phase);
}