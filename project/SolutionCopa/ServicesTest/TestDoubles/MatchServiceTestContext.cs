using DataAccess.Interfaces;
using ProjectCopa;
using Services;
using Services.DTOs;
using ServicesTest.TestDoubles.Fakes_Stubs;

namespace ServicesTest.TestDoubles;

public sealed class MatchServiceTestContext
{
    public FakeMatchRepository MatchRepository { get; } = new();
    
    // single multipurpose lookup repository used for teams, stadiums, groups, journeys and champ confs
    private readonly MultiFakeLookupRepository _lookup = new();
    private ITeamRepository TeamRepository { get; }

    private IStadiumRepository StadiumRepository { get; }

    private IGroupRepository GroupRepository { get; }

    private IJourneyRepository JourneyRepository { get; }

    private IChampConfRepository ChampConfRepository { get; }

    public FakeUnitOfWork UnitOfWork { get; } = new();
    public MatchService Service { get; }

    public MatchServiceTestContext()
    {
        _lookup.Set(10, new Team(10, "Home Team"));
        _lookup.Set(20, new Team(20, "Away Team"));
        _lookup.Set(30, new Stadium(30, "Main Stadium"));
        _lookup.Set(40, new Group("A"));
        _lookup.Set(50, new Journey(1));
        _lookup.Set(60, new ChampConf(new DateTime(2026, 6, 1)));

        // adapters delegate to the shared lookup repository
        TeamRepository = new TeamAdapter(_lookup);
        StadiumRepository = new StadiumAdapter(_lookup);
        GroupRepository = new GroupAdapter(_lookup);
        JourneyRepository = new JourneyAdapter(_lookup);
        ChampConfRepository = new ChampConfAdapter(_lookup);

        Service = new MatchService(
            MatchRepository,
            TeamRepository,
            StadiumRepository,
            GroupRepository,
            JourneyRepository,
            ChampConfRepository,
            UnitOfWork);
    }

    // simple adapters that forward GetById to the shared lookup store
    private sealed class TeamAdapter(MultiFakeLookupRepository lookup) : ITeamRepository
    {
        public Team? GetById(int teamId) => lookup.GetByIdTyped<Team>(teamId);
    }

    private sealed class StadiumAdapter(MultiFakeLookupRepository lookup) : IStadiumRepository
    {
        public Stadium? GetById(int stadiumId) => lookup.GetByIdTyped<Stadium>(stadiumId);
    }

    private sealed class GroupAdapter(MultiFakeLookupRepository lookup) : IGroupRepository
    {
        public Group? GetById(int groupId) => lookup.GetByIdTyped<Group>(groupId);
    }

    private sealed class JourneyAdapter(MultiFakeLookupRepository lookup) : IJourneyRepository
    {
        public Journey? GetById(int journeyId) => lookup.GetByIdTyped<Journey>(journeyId);
    }

    private sealed class ChampConfAdapter(MultiFakeLookupRepository lookup) : IChampConfRepository
    {
        public ChampConf? GetById(int championshipConfigurationId) => lookup.GetByIdTyped<ChampConf>(championshipConfigurationId);
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
            Phase = Phase.RoundOf16,
            MatchCode = "A1",
            MatchHomeId = 100,
            MatchAwayId = 200
        };
    }

    public void AddSourceEliminationMatches()
    {
        MatchRepository.Add(new StubMatch(100, new DateTime(2026, 6, 20), new Team(1, "Team 1"), new Team(2, "Team 2"), new Stadium(1, "Stadium 1")));
        MatchRepository.Add(new StubMatch(200, new DateTime(2026, 6, 20), new Team(3, "Team 3"), new Team(4, "Team 4"), new Stadium(2, "Stadium 2")));
    }
}
