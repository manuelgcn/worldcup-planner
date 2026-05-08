using ProjectCopa;

namespace DomainTest.TestDoubles;

public sealed class MockMatch : Match
{
    public MockMatch()
    {
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
