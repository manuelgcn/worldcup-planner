namespace Services.DTOs;

public sealed class TeamImportDto
{
    public int RowNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Confederation { get; init; } = string.Empty;
    public string Ranking { get; init; } = string.Empty;
}
