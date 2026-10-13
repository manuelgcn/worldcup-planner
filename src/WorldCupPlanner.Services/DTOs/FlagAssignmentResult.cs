namespace WorldCupPlanner.Services.DTOs;

public sealed class FlagAssignmentResult
{
    public FlagAssignmentResult(int assignedCount, int skippedExistingCount, IReadOnlyCollection<string> unmappedTeamNames)
    {
        AssignedCount = assignedCount;
        SkippedExistingCount = skippedExistingCount;
        UnmappedTeamNames = unmappedTeamNames;
    }

    public int AssignedCount { get; }

    public int SkippedExistingCount { get; }

    public IReadOnlyCollection<string> UnmappedTeamNames { get; }
}
