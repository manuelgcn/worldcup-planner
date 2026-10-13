namespace WorldCupPlanner.Services.DTOs;

public sealed class MatchIncidentDto
{
    public const string YellowCardType = "YellowCard";
    public const string RedCardType = "RedCard";

    public int Id { get; set; }
    public int Minute { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
