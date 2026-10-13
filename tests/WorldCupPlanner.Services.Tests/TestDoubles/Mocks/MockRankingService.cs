using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

public sealed class MockRankingService : IRankingService
{
    public int ApplyMatchResultCalls { get; private set; }
    public int RevertMatchResultCalls { get; private set; }

    public void ApplyMatchResult(Match match, MatchPhase phase)
    {
        ApplyMatchResultCalls++;
    }

    public void RevertMatchResult(Match match)
    {
        RevertMatchResultCalls++;
    }
}
