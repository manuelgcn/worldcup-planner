using ProjectCopa;

namespace DomainTest;

public sealed class TestMatch : Match
{
    public TestMatch()
    {
    }

    public TestMatch(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }

    public TestMatch(
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
