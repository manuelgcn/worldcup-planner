namespace WorldCupPlanner.Domain;

public sealed class RedCardIncident : MatchIncident
{
    private RedCardIncident()
    {
    }

    public RedCardIncident(int id, int minute, Team team)
        : base(id, minute, team)
    {
    }
}
