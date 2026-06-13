using ProjectCopa;

namespace Services.DTOs;

public sealed class ImportResult
{
    public ImportResult(IReadOnlyCollection<Team> importedTeams, string? warningMessage = null)
    {
        ImportedTeams = importedTeams;
        WarningMessage = warningMessage;
    }

    public IReadOnlyCollection<Team> ImportedTeams { get; }

    public int ImportedCount => ImportedTeams.Count;

    public string? WarningMessage { get; }
}