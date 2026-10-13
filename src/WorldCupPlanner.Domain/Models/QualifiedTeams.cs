namespace WorldCupPlanner.Domain.Models;

public record QualifiedTeams
{
    public List<Team> TopEightFirsts { get; init; } = new();

    public List<Team> RemainingFourFirsts { get; init; } = new();

    public List<Team> TopEightSeconds { get; init; } = new();

    public List<Team> WorstFourSeconds { get; init; } = new();

    public List<Team> BestEightThirds { get; init; } = new();

    public Dictionary<int, string> GroupByTeamId { get; init; } = new();
}
