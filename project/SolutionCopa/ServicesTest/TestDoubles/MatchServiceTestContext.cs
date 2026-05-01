using Services;
using Services.DTOs;

namespace ServicesTest.TestDoubles;

public sealed class MatchServiceTestContext
{
    public FakeMatchRepository MatchRepository { get; } = new();
    // single multi-purpose lookup repository used for teams, stadiums, groups, journeys and champ confs
    private readonly MultiFakeLookupRepository _lookup = new();
    private readonly DataAccess.Interfaces.ITeamRepository _teamAdapter;
    private readonly DataAccess.Interfaces.IStadiumRepository _stadiumAdapter;
    private readonly DataAccess.Interfaces.IGroupRepository _groupAdapter;
    private readonly DataAccess.Interfaces.IJourneyRepository _journeyAdapter;
    private readonly DataAccess.Interfaces.IChampConfRepository _champConfAdapter;
    public DataAccess.Interfaces.ITeamRepository TeamRepository => _teamAdapter;
    public DataAccess.Interfaces.IStadiumRepository StadiumRepository => _stadiumAdapter;
    public DataAccess.Interfaces.IGroupRepository GroupRepository => _groupAdapter;
    public DataAccess.Interfaces.IJourneyRepository JourneyRepository => _journeyAdapter;
    public DataAccess.Interfaces.IChampConfRepository ChampConfRepository => _champConfAdapter;
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public MatchService Service { get; }

    public MatchServiceTestContext()
    {
        _lookup.Set<ProjectCopa.Interfaces.ITeam>(10, new StubTeam());
        _lookup.Set<ProjectCopa.Interfaces.ITeam>(20, new StubTeam());
        _lookup.Set<ProjectCopa.Interfaces.IStadium>(30, new StubStadium());
        _lookup.Set<ProjectCopa.Interfaces.IGroup>(40, new StubGroup("A"));
        _lookup.Set<ProjectCopa.Interfaces.IJourney>(50, new StubJourney(1));
        _lookup.Set<ProjectCopa.Interfaces.IChampConf>(60, new StubChampConf(new DateTime(2026, 6, 1)));

        // adapters delegate to the shared lookup repository
        _teamAdapter = new TeamAdapter(_lookup);
        _stadiumAdapter = new StadiumAdapter(_lookup);
        _groupAdapter = new GroupAdapter(_lookup);
        _journeyAdapter = new JourneyAdapter(_lookup);
        _champConfAdapter = new ChampConfAdapter(_lookup);

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
    private sealed class TeamAdapter : DataAccess.Interfaces.ITeamRepository
    {
        private readonly MultiFakeLookupRepository _lookup;
        public TeamAdapter(MultiFakeLookupRepository lookup) => _lookup = lookup;
        public ProjectCopa.Interfaces.ITeam? GetById(int teamId) => _lookup.GetByIdTyped<ProjectCopa.Interfaces.ITeam>(teamId);
    }

    private sealed class StadiumAdapter : DataAccess.Interfaces.IStadiumRepository
    {
        private readonly MultiFakeLookupRepository _lookup;
        public StadiumAdapter(MultiFakeLookupRepository lookup) => _lookup = lookup;
        public ProjectCopa.Interfaces.IStadium? GetById(int stadiumId) => _lookup.GetByIdTyped<ProjectCopa.Interfaces.IStadium>(stadiumId);
    }

    private sealed class GroupAdapter : DataAccess.Interfaces.IGroupRepository
    {
        private readonly MultiFakeLookupRepository _lookup;
        public GroupAdapter(MultiFakeLookupRepository lookup) => _lookup = lookup;
        public ProjectCopa.Interfaces.IGroup? GetById(int groupId) => _lookup.GetByIdTyped<ProjectCopa.Interfaces.IGroup>(groupId);
    }

    private sealed class JourneyAdapter : DataAccess.Interfaces.IJourneyRepository
    {
        private readonly MultiFakeLookupRepository _lookup;
        public JourneyAdapter(MultiFakeLookupRepository lookup) => _lookup = lookup;
        public ProjectCopa.Interfaces.IJourney? GetById(int journeyId) => _lookup.GetByIdTyped<ProjectCopa.Interfaces.IJourney>(journeyId);
    }

    private sealed class ChampConfAdapter : DataAccess.Interfaces.IChampConfRepository
    {
        private readonly MultiFakeLookupRepository _lookup;
        public ChampConfAdapter(MultiFakeLookupRepository lookup) => _lookup = lookup;
        public ProjectCopa.Interfaces.IChampConf? GetById(int championshipConfigurationId) => _lookup.GetByIdTyped<ProjectCopa.Interfaces.IChampConf>(championshipConfigurationId);
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
