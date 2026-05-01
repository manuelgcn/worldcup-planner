using ServicesTest.TestDoubles.Fakes_Stubs;
using Services;
using Services.DTOs;
using DataAccess.Interfaces;
using ProjectCopa.Interfaces;

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
        _lookup.Set<ITeam>(10, new StubTeam());
        _lookup.Set<ITeam>(20, new StubTeam());
        _lookup.Set<IStadium>(30, new StubStadium());
        _lookup.Set<IGroup>(40, new StubGroup("A"));
        _lookup.Set<IJourney>(50, new StubJourney(1));
        _lookup.Set<IChampConf>(60, new StubChampConf(new DateTime(2026, 6, 1)));

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
        public ITeam? GetById(int teamId) => lookup.GetByIdTyped<ITeam>(teamId);
    }

    private sealed class StadiumAdapter(MultiFakeLookupRepository lookup) : IStadiumRepository
    {
        public IStadium? GetById(int stadiumId) => lookup.GetByIdTyped<IStadium>(stadiumId);
    }

    private sealed class GroupAdapter(MultiFakeLookupRepository lookup) : IGroupRepository
    {
        public IGroup? GetById(int groupId) => lookup.GetByIdTyped<IGroup>(groupId);
    }

    private sealed class JourneyAdapter(MultiFakeLookupRepository lookup) : IJourneyRepository
    {
        public IJourney? GetById(int journeyId) => lookup.GetByIdTyped<IJourney>(journeyId);
    }

    private sealed class ChampConfAdapter(MultiFakeLookupRepository lookup) : IChampConfRepository
    {
        public IChampConf? GetById(int championshipConfigurationId) => lookup.GetByIdTyped<IChampConf>(championshipConfigurationId);
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
