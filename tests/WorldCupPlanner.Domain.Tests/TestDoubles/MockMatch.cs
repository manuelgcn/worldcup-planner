using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests.TestDoubles;

public sealed class MockMatch : Match
{
    public MockMatch()
    {
    }

    public static bool CanUseSourceMatches(Match? homeMatch, Match? awayMatch)
    {
        EnsureDifferentSourceMatches(homeMatch, awayMatch);
        return true;
    }

    public MockMatch(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }

    public MockMatch(
        int id,
        DateTime matchDate,
        Team homeTeam,
        Team awayTeam,
        Stadium stadium,
        TournamentSettings tournamentSettings)
        : base(id, matchDate, homeTeam, awayTeam, stadium, tournamentSettings)
    {
    }
}
