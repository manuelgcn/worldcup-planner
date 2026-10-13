namespace WorldCupPlanner.Domain.Models;

public sealed class GroupStanding
{
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;

    public GroupStanding(Team team, int tieBreaker)
    {
        Team = team;
        TieBreaker = tieBreaker;
    }

    public Team Team { get; }
    public int TieBreaker { get; }
    public int GamesPlayed { get; private set; }
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }

    public int GoalDifference => GoalsFor - GoalsAgainst;

    public int Points => Wins * PointsForWin + Draws * PointsForDraw;

    internal void RegisterResult(int goalsScored, int goalsConceded)
    {
        GamesPlayed++;
        GoalsFor += goalsScored;
        GoalsAgainst += goalsConceded;

        if (goalsScored > goalsConceded)
        {
            Wins++;
        }
        else if (goalsScored == goalsConceded)
        {
            Draws++;
        }
        else
        {
            Losses++;
        }
    }
}
