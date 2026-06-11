using Services.DTOs;

namespace Services.Interfaces;

public interface IFlagAssignmentService
{
    FlagAssignmentResult AssignFlags(bool overwriteExisting = false, string? performedByEmail = null);
}
