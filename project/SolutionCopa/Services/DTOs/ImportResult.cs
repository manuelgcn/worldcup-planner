using ProjectCopa;

namespace Services.DTOs;

public sealed class ImportResult
{
    public ImportResult(IReadOnlyCollection<Team> importedTeams)
    {
        ImportedTeams = importedTeams;
    }

    public IReadOnlyCollection<Team> ImportedTeams { get; }

    public int ImportedCount => ImportedTeams.Count;
}
