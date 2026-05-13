using ProjectCopa;

namespace ProjectCopa.Models;

public record QualifiedTeams
{
    public List<Team> TopEightFirsts { get; init; } = new List<Team>();

    public List<Team> RemainingFourFirsts { get; init; } = new List<Team>();

    public List<Team> TopEightSeconds { get; init; } = new List<Team>();

    public List<Team> WorstFourSeconds { get; init; } = new List<Team>();

    public List<Team> BestEightThirds { get; init; } = new List<Team>();
}
