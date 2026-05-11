using Services.DTOs;

namespace Services.Interfaces;

public interface ITeamParser
{
    List<TeamImportDto> Parse(Stream fileStream);
}
