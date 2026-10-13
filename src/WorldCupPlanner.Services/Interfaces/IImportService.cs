using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IImportService
{
    ImportResult ImportTeams(Stream fileStream, string? performedByEmail = null);
}
