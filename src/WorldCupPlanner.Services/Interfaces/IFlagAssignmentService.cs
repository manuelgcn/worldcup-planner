using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IFlagAssignmentService
{
    FlagAssignmentResult AssignFlags(bool overwriteExisting = false, string? performedByEmail = null);
}
