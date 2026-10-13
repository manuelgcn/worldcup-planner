using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface ITeamParser
{
    List<TeamImportDto> Parse(Stream fileStream);
}
