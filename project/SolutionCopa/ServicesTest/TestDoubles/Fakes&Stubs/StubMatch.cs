using ProjectCopa;
using System;

namespace ServicesTest.TestDoubles.Fakes_Stubs;

public sealed class StubMatch : Match
{
    public StubMatch(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
        : base(id, matchDate, homeTeam, awayTeam, stadium)
    {
    }
}
