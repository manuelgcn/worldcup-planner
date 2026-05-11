using Services.DTOs;

namespace Services.Interfaces;

public interface IImportService
{
    ImportResult ImportTeams(Stream fileStream);
}
