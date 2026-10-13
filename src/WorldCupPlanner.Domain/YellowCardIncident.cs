namespace WorldCupPlanner.Domain;

public sealed class YellowCardIncident : MatchIncident
{
    private YellowCardIncident()
    {
    }

    public YellowCardIncident(int id, int minute, Team team)
        : base(id, minute, team)
    {
    }
}
