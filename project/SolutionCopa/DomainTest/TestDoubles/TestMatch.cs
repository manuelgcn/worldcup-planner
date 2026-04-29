using ProjectCopa;
using ProjectCopa.Interfaces;

namespace DomainTest;

public sealed class TestMatch : Match
{
    public TestMatch()
    {
    }

    public TestMatch(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }

    public TestMatch(
        int id,
        DateTime matchDate,
        ITeam homeTeam,
        ITeam awayTeam,
        IStadium stadium,
        IChampConf championshipConfiguration)
        : base(id, matchDate, homeTeam, awayTeam, stadium, championshipConfiguration)
    {
    }
}
