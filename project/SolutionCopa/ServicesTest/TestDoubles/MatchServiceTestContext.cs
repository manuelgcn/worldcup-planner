using Services;
using Services.DTOs;

namespace ServicesTest.TestDoubles;

public sealed class MatchServiceTestContext
{
    public FakeMatchRepository MatchRepository { get; } = new();
    public FakeTeamRepository TeamRepository { get; } = new();
    public FakeStadiumRepository StadiumRepository { get; } = new();
    public FakeGroupRepository GroupRepository { get; } = new();
    public FakeJourneyRepository JourneyRepository { get; } = new();
    public FakeChampConfRepository ChampConfRepository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public MatchService Service { get; }

    public MatchServiceTestContext()
    {
        TeamRepository.Set(10, new StubTeam());
        TeamRepository.Set(20, new StubTeam());
        StadiumRepository.Set(30, new StubStadium());
        GroupRepository.Set(40, new StubGroup("A"));
        JourneyRepository.Set(50, new StubJourney(1));
        ChampConfRepository.Set(60, new StubChampConf(new DateTime(2026, 6, 1)));

        Service = new MatchService(
            MatchRepository,
            TeamRepository,
            StadiumRepository,
            GroupRepository,
            JourneyRepository,
            ChampConfRepository,
            UnitOfWork);
    }

    public CreateMatchRequestDto NewGroupMatchRequest(int id = 1)
    {
        return new CreateMatchRequestDto
        {
            MatchId = id,
            MatchDate = new DateTime(2026, 6, 21),
            HomeTeamId = 10,
            AwayTeamId = 20,
            StadiumId = 30,
            GroupId = 40,
            JourneyId = 50
        };
    }

    public CreateMatchRequestDto NewEliminationMatchRequest(int id = 2)
    {
        return new CreateMatchRequestDto
        {
            MatchId = id,
            MatchDate = new DateTime(2026, 6, 25),
            HomeTeamId = 10,
            AwayTeamId = 20,
            StadiumId = 30,
            IsElimination = true,
            ChampionshipConfigurationId = 60,
            Phase = ProjectCopa.Phase.RoundOf16,
            MatchCode = "A1",
            MatchHomeId = 100,
            MatchAwayId = 200
        };
    }

    public void AddSourceEliminationMatches()
    {
        MatchRepository.Add(new StubMatch(100, new DateTime(2026, 6, 20), new StubTeam(), new StubTeam(), new StubStadium()));
        MatchRepository.Add(new StubMatch(200, new DateTime(2026, 6, 20), new StubTeam(), new StubTeam(), new StubStadium()));
    }
}
