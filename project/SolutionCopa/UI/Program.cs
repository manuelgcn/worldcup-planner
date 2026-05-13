using Services;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using UI;
using UI.Components;
using ProjectCopa;
using Services.Interfaces;
using Services.Parsers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSingleton<ITeamRepository, TeamRepository>();
builder.Services.AddSingleton<IMatchRepository, MatchRepository>();
builder.Services.AddSingleton<IStadiumRepository, StadiumRepository>();
builder.Services.AddSingleton<IGroupRepository, GroupRepository>();
builder.Services.AddSingleton<IJourneyRepository, JourneyRepository>();
builder.Services.AddSingleton<ITournamentSettingsRepository, TournamentSettingsRepository>();
builder.Services.AddSingleton<IEliminationMatchRepository, EliminationMatchRepository>();
builder.Services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWorkRepository>();


builder.Services.AddSingleton<UserServices>();
builder.Services.AddSingleton<TeamServices>();
builder.Services.AddSingleton<StadiumServices>();
builder.Services.AddSingleton<IStadiumService>(sp => sp.GetRequiredService<StadiumServices>());
builder.Services.AddSingleton<GroupServices>();
builder.Services.AddSingleton<IGroupService>(sp => sp.GetRequiredService<GroupServices>());
builder.Services.AddSingleton<JourneyServices>();
builder.Services.AddSingleton<IJourneyService>(sp => sp.GetRequiredService<JourneyServices>());


builder.Services.AddSingleton<IMatchSimulationEngine, MatchSimulationEngine>();
builder.Services.AddSingleton<IAuditLogService>(sp =>
    AuditLogService.CreateInstance(
        sp.GetRequiredService<IAuditLogRepository>(),
        defaultTournamentSettingsId: 1));
builder.Services.AddSingleton<KnockoutFixtureGenerator>(sp =>
    new KnockoutFixtureGenerator(
        sp.GetRequiredService<IMatchSimulationEngine>(),
        sp.GetRequiredService<IMatchRepository>(),
        sp.GetRequiredService<IEliminationMatchRepository>(),
        sp.GetRequiredService<IGroupRepository>(),
        sp.GetRequiredService<IStadiumRepository>(),
        sp.GetRequiredService<ITournamentSettingsRepository>(),
        sp.GetRequiredService<IUnitOfWork>(),
        sp.GetRequiredService<IAuditLogService>()));
builder.Services.AddSingleton<IKnockoutFixtureGenerator>(sp => sp.GetRequiredService<KnockoutFixtureGenerator>());
builder.Services.AddSingleton<ITournamentSimulationService, TournamentSimulationService>();
builder.Services.AddSingleton<IFixtureOrchestrator, FixtureOrchestrator>();
builder.Services.AddSingleton<IGroupStandingsService, GroupStandingsService>();
builder.Services.AddSingleton<ITeamParser, CsvStreamTeamParser>();
builder.Services.AddSingleton<IImportService>(sp =>
    ImportService.CreateInstance(
        sp.GetRequiredService<ITeamRepository>(),
        sp.GetRequiredService<IUnitOfWork>(),
        sp.GetRequiredService<ITeamParser>()));


builder.Services.AddScoped<SessionState>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// =======================
// Seed default TournamentSettings (id=1)
// Required because IAuditLogService is created with defaultTournamentSettingsId=1.
// =======================
ITournamentSettingsRepository settingsRepository =
    app.Services.GetRequiredService<ITournamentSettingsRepository>();

const int DefaultTournamentSettingsId = 1;

if (settingsRepository.GetById(DefaultTournamentSettingsId) is null)
{
    TournamentSettings defaultSettings = new TournamentSettings(
        id: DefaultTournamentSettingsId,
        startDate: new DateTime(2026, 6, 1),
        maxMatchesPerDay: 3,
        daysBetweenJourneys: 3,
        fixtureSeed: 0,
        crossingsSeed: 0,
        simulationSeed: 0,
        completionSeed: 0);

    settingsRepository.Add(defaultSettings);
}

// =======================
// Seed Admin
// =======================
UserServices userServices = app.Services.GetRequiredService<UserServices>();

User admin = new User(
    1,
    "Admin",
    "Sistema",
    "admin@copa.com",
    "Admin@123",
    new DateTime(1990, 1, 1)
);

admin.AddRole(UserRoles.Admin);
userServices.AddUser(admin);


User editor = new User(
    2,
    "Editor",
    "Sistema",
    "editor@copa.com",
    "Editor@123",
    new DateTime(1995, 5, 15)
);
editor.AddRole(UserRoles.Editor);
userServices.AddUser(editor);



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();