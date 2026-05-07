using ProjectCopa;
using System;

namespace ServiceTest.TestDoubles.Fakes_Stubs;

public sealed class MockMatch(int id, DateTime matchDate, Team homeTeam, Team awayTeam, Stadium stadium)
    : Match(id, matchDate, homeTeam, awayTeam, stadium);
