using ProjectCopa;
using Services.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

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
