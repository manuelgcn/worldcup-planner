using DataAccess.Interfaces;
using ProjectCopa;
using Services.Exceptions;
using Services.Interfaces;

namespace Services;

public sealed class FixtureOrchestrator : IFixtureOrchestrator
{
    private const int RequiredTeamCount = 48;
    private const int MinimumStadiumCount = 4;
    private const int GroupStageJourneyCount = 3;
    private const string FixtureAlreadyGeneratedMessage = "Fixture is already generated. Use RegenerateGroupStage instead.";
    private const string KnockoutAlreadyGeneratedMessage = "Cannot regenerate fixture: knockout phase already exists.";
    private const string MissingSettingsMessage = "Tournament settings are not configured.";
    private const string MissingTeamsMessage = "Fixture requires exactly 48 teams.";
    private const string MissingStadiumsMessage = "Fixture requires at least 4 stadiums.";
    private const string FixtureNotGeneratedMessage = "Fixture has not been generated yet.";

    private readonly ITeamRepository _teamRepository;
    private readonly IStadiumRepository _stadiumRepository;
    private readonly ITournamentSettingsRepository _settingsRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IEliminationMatchRepository _eliminationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public FixtureOrchestrator(
        ITeamRepository teamRepository,
        IStadiumRepository stadiumRepository,
        ITournamentSettingsRepository settingsRepository,
        IGroupRepository groupRepository,
        IJourneyRepository journeyRepository,
        IMatchRepository matchRepository,
        IEliminationMatchRepository eliminationRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _teamRepository = teamRepository;
        _stadiumRepository = stadiumRepository;
        _settingsRepository = settingsRepository;
        _groupRepository = groupRepository;
        _journeyRepository = journeyRepository;
        _matchRepository = matchRepository;
        _eliminationRepository = eliminationRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public bool IsFixtureGenerated()
    {
        return _groupRepository.GetAll().Count > 0;
    }

    public bool IsKnockoutGenerated()
    {
        return _eliminationRepository.GetAll().Count > 0;
    }

    public void GenerateGroupStage(int? fixtureSeedOverride, string? performedByEmail)
    {
        EnsureFixtureNotYetGenerated();
        TournamentSettings settings = LoadSettings();
        IReadOnlyCollection<Team> teams = LoadTeams();
        IReadOnlyCollection<Stadium> stadiums = LoadStadiums();

        int effectiveSeed = ResolveFixtureSeed(fixtureSeedOverride, settings);

        BuildAndPersistFixture(teams, stadiums, settings, effectiveSeed);

        _unitOfWork.Commit();

        _auditLogService.LogPerformedBy(
            AuditActions.GenerateFixture,
            $"Fixture generated with seed {effectiveSeed}.",
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    public void RegenerateGroupStage(int? fixtureSeedOverride, string? performedByEmail)
    {
        EnsureFixtureGenerated();
        EnsureKnockoutNotYetGenerated();

        TournamentSettings settings = LoadSettings();
        IReadOnlyCollection<Team> teams = LoadTeams();
        IReadOnlyCollection<Stadium> stadiums = LoadStadiums();
        int effectiveSeed = ResolveFixtureSeed(fixtureSeedOverride, settings);

        ClearGroupStageStorage();
        _auditLogService.LogPerformedBy(
            AuditActions.ClearFixture,
            "Previous fixture cleared before regeneration.",
            performedByEmail,
            nameof(FixtureOrchestrator));

        BuildAndPersistFixture(teams, stadiums, settings, effectiveSeed);

        _unitOfWork.Commit();

        _auditLogService.LogPerformedBy(
            AuditActions.RegenerateFixture,
            $"Fixture regenerated with seed {effectiveSeed}.",
            performedByEmail,
            nameof(FixtureOrchestrator));
    }

    private void EnsureFixtureNotYetGenerated()
    {
        if (IsFixtureGenerated())
        {
            throw new FixtureOrchestratorException(FixtureAlreadyGeneratedMessage);
        }
    }

    private void EnsureFixtureGenerated()
    {
        if (!IsFixtureGenerated())
        {
            throw new FixtureOrchestratorException(FixtureNotGeneratedMessage);
        }
    }

    private void EnsureKnockoutNotYetGenerated()
    {
        if (IsKnockoutGenerated())
        {
            throw new FixtureOrchestratorException(KnockoutAlreadyGeneratedMessage);
        }
    }

    private TournamentSettings LoadSettings()
    {
        TournamentSettings? settings = _settingsRepository.GetAll().FirstOrDefault();
        if (settings == null)
        {
            throw new FixtureOrchestratorException(MissingSettingsMessage);
        }
        return settings;
    }

    private IReadOnlyCollection<Team> LoadTeams()
    {
        IReadOnlyCollection<Team> teams = _teamRepository.GetAll();
        if (teams.Count != RequiredTeamCount)
        {
            throw new FixtureOrchestratorException(MissingTeamsMessage);
        }
        return teams;
    }

    private IReadOnlyCollection<Stadium> LoadStadiums()
    {
        IReadOnlyCollection<Stadium> stadiums = _stadiumRepository.GetAll();
        if (stadiums.Count < MinimumStadiumCount)
        {
            throw new FixtureOrchestratorException(MissingStadiumsMessage);
        }
        return stadiums;
    }

    private static int ResolveFixtureSeed(int? fixtureSeedOverride, TournamentSettings settings)
    {
        return fixtureSeedOverride ?? settings.FixtureSeed;
    }

    private void BuildAndPersistFixture(
        IReadOnlyCollection<Team> teams,
        IReadOnlyCollection<Stadium> stadiums,
        TournamentSettings settings,
        int effectiveSeed)
    {
        FixtureService fixtureService = new FixtureService(teams, stadiums, effectiveSeed, auditLogService: null);
        IReadOnlyCollection<Group> groups = fixtureService.CreateFixture();

        PersistGroups(groups);
        PersistJourneys();
        PersistMatches(groups, stadiums, settings);
    }

    private void PersistGroups(IReadOnlyCollection<Group> groups)
    {
        foreach (Group group in groups)
        {
            _groupRepository.Add(group);
        }
    }

    private void PersistJourneys()
    {
        for (int journeyNumber = 1; journeyNumber <= GroupStageJourneyCount; journeyNumber++)
        {
            _journeyRepository.Add(Journey.CreateInstance(journeyNumber));
        }
    }

    private void PersistMatches(
        IReadOnlyCollection<Group> groups,
        IReadOnlyCollection<Stadium> stadiums,
        TournamentSettings settings)
    {
        GroupFixtureGenerator matchGenerator = new GroupFixtureGenerator();
        List<Match> matches = matchGenerator.GenerateMatches(
            groups.ToList(),
            stadiums.ToList(),
            settings.StartDate);

        foreach (Match match in matches)
        {
            _matchRepository.Add(match);
        }
    }

    private void ClearGroupStageStorage()
    {
        _matchRepository.Clear();
        _journeyRepository.Clear();
        _groupRepository.Clear();
    }
}
