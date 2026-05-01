using ProjectCopa;
using ProjectCopa.Interfaces;

namespace ServicesTest.TestDoubles;

public sealed class StubMatch : Match
{
    public StubMatch(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }
}
