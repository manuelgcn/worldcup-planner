using ProjectCopa;
using ProjectCopa.Models;
using Services.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockKnockoutFixtureGenerator : IKnockoutFixtureGenerator
{
    private readonly List<Match> _matchesToReturn = new List<Match>();

    public bool GeneratePhase2WasCalled { get; private set; }

    public QualifiedTeams? LastQualifiedTeamsReceived { get; private set; }

    public int GeneratePhase2Calls { get; private set; }

    public void SeedResult(IEnumerable<Match> matches)
    {
        _matchesToReturn.Clear();
        _matchesToReturn.AddRange(matches);
    }

    public List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams)
    {
        GeneratePhase2WasCalled = true;
        GeneratePhase2Calls++;
        LastQualifiedTeamsReceived = qualifiedTeams;
        return new List<Match>(_matchesToReturn);
    }
}
