using ProjectCopa;
using ProjectCopa.Interfaces;
using System;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

public sealed class StubMatch : Match
{
    public StubMatch(int id, DateTime matchDate, ITeam homeTeam, ITeam awayTeam, IStadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }
}
