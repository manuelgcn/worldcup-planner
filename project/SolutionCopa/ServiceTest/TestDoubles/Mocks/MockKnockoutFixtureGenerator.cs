using ProjectCopa;
using ProjectCopa.Models;
using Services.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

public sealed class MockKnockoutFixtureGenerator : IKnockoutFixtureGenerator
{
    public QualifiedTeams? LastQualifiedTeamsReceived { get; private set; }
    public int GeneratePhase2Calls { get; private set; }
    public int GenerateBracketCalls { get; private set; }
    public int SimulateNextRoundCalls { get; private set; }
    public int SimulateUntilFinalCalls { get; private set; }
    public bool BracketGenerated { get; set; }

    public bool IsBracketGenerated() => BracketGenerated;

    public void GenerateBracket(int? crossingsSeedOverride, string? performedByEmail)
    {
        GenerateBracketCalls++;
        BracketGenerated = true;
    }

    public void SimulateNextRound(string? performedByEmail)
    {
        SimulateNextRoundCalls++;
    }

    public void SimulateUntilFinal(string? performedByEmail)
    {
        SimulateUntilFinalCalls++;
    }

    public List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams)
    {
        LastQualifiedTeamsReceived = qualifiedTeams;
        GeneratePhase2Calls++;

        List<Team> teams = qualifiedTeams.TopEightFirsts
            .Concat(qualifiedTeams.RemainingFourFirsts)
            .Concat(qualifiedTeams.TopEightSeconds)
            .Concat(qualifiedTeams.WorstFourSeconds)
            .Concat(qualifiedTeams.BestEightThirds)
            .ToList();

        List<Match> matches = new List<Match>();
        Stadium stadium = new Stadium("Knockout Stadium", "City", null, 30000) { Id = 999 };
        Group group = new Group("Z");
        Journey journey = Journey.CreateInstance(1);

        for (int index = 0; index < 16; index++)
        {
            Team home = teams[index * 2];
            Team away = teams[(index * 2) + 1];
            MatchGroup sourceHome = MatchGroup.CreateInstance(-(index * 2) - 1, DateTime.UtcNow, home, away, stadium, group, journey);
            MatchGroup sourceAway = MatchGroup.CreateInstance(-(index * 2) - 2, DateTime.UtcNow, away, home, stadium, group, journey);
            MatchElimination elimination = MatchElimination.Create(
                4000 + index,
                DateTime.UtcNow.AddDays(index),
                home,
                away,
                stadium,
                Phase.RoundOf32,
                $"R32-{index + 1}",
                sourceHome,
                sourceAway);
            matches.Add(elimination);
        }

        return matches;
    }
}
