using WorldCupPlanner.Services;
using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.UI;
using WorldCupPlanner.UI.Components;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Parsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<ITeamRepository, TeamRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
builder.Services.AddSingleton<IMatchRepository, MatchRepository>();
builder.Services.AddSingleton<IStadiumRepository, StadiumRepository>();
builder.Services.AddSingleton<IGroupRepository, GroupRepository>();
builder.Services.AddSingleton<IJourneyRepository, JourneyRepository>();
builder.Services.AddSingleton<ITournamentSettingsRepository, TournamentSettingsRepository>();
builder.Services.AddSingleton<IEliminationMatchRepository, EliminationMatchRepository>();
builder.Services.AddSingleton<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddSingleton<UserServices>();
builder.Services.AddSingleton<IUserService>(sp => sp.GetRequiredService<UserServices>());

builder.Services.AddSingleton<TeamServices>();
builder.Services.AddSingleton<ITeamService>(sp => sp.GetRequiredService<TeamServices>());
builder.Services.AddSingleton<StadiumServices>();
builder.Services.AddSingleton<IStadiumService>(sp => sp.GetRequiredService<StadiumServices>());
builder.Services.AddSingleton<GroupServices>();
builder.Services.AddSingleton<IGroupService>(sp => sp.GetRequiredService<GroupServices>());
builder.Services.AddSingleton<JourneyServices>();
builder.Services.AddSingleton<IJourneyService>(sp => sp.GetRequiredService<JourneyServices>());
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IExporter, Exporter>();
builder.Services.AddSingleton<IExportService, ExportService>();

builder.Services.AddSingleton<IMatchSimulationEngine, ProbabilisticSimulationEngine>();
builder.Services.AddSingleton<IMatchSimulationEngine, PureRandomSimulationEngine>(); // OCP: nuevos motores se agregan aca sin tocar consumidores
builder.Services.AddSingleton<ISimulationEngineRegistry, SimulationEngineRegistry>();
builder.Services.AddSingleton<IAuditLogService>(sp =>
    AuditLogService.CreateInstance(sp.GetRequiredService<IAuditLogRepository>()));

builder.Services.AddSingleton<IRankingService, RankingService>();
builder.Services.AddSingleton<IMatchFactory, MatchFactory>();
builder.Services.AddSingleton<IMatchService, MatchService>();
builder.Services.AddSingleton<IMatchQueryService, MatchQueryService>();
builder.Services.AddSingleton<ITournamentDashboardService, TournamentDashboardService>();
builder.Services.AddSingleton<ISimulationConfigurationService, SimulationConfigurationService>();
builder.Services.AddSingleton<IFixtureViewService, FixtureViewService>();


builder.Services.AddSingleton<KnockoutFixtureGenerator>(sp =>
    new KnockoutFixtureGenerator(
        sp.GetRequiredService<ISimulationEngineRegistry>(),
        sp.GetRequiredService<IMatchRepository>(),
        sp.GetRequiredService<IEliminationMatchRepository>(),
        sp.GetRequiredService<IGroupRepository>(),
        sp.GetRequiredService<IStadiumRepository>(),
        sp.GetRequiredService<ITournamentSettingsRepository>(),
        sp.GetRequiredService<IAuditLogService>(),
        sp.GetRequiredService<INotificationService>(),
        sp.GetRequiredService<IRankingService>()));

builder.Services.AddSingleton<IKnockoutFixtureGenerator>(sp =>
    sp.GetRequiredService<KnockoutFixtureGenerator>());

builder.Services.AddSingleton<ITournamentSimulationService, TournamentSimulationService>();
builder.Services.AddSingleton<IFixtureOrchestrator, FixtureOrchestrator>();
builder.Services.AddSingleton<IGroupStandingsService, GroupStandingsService>();
builder.Services.AddSingleton<ITeamParser, CsvStreamTeamParser>();

builder.Services.AddSingleton<IImportService>(sp =>
    ImportService.CreateInstance(
        sp.GetRequiredService<ITeamRepository>(),
        sp.GetRequiredService<ITeamParser>(),
        sp.GetRequiredService<IAuditLogService>()));

string flagsDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration.GetValue<string>("Flags:Directory") ?? "flags-master");

builder.Services.AddSingleton<IFlagAssignmentService>(sp =>
    new FlagAssignmentService(
        sp.GetRequiredService<ITeamRepository>(),
        flagsDirectory,
        sp.GetRequiredService<IAuditLogService>()));

builder.Services.AddScoped<SessionState>();

var app = builder.Build();

using (var db = app.Services
           .GetRequiredService<IDbContextFactory<AppDbContext>>()
           .CreateDbContext())
{
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

var settingsRepository = app.Services.GetRequiredService<ITournamentSettingsRepository>();
var settingsSection = builder.Configuration.GetSection("TournamentSettings");
int settingsId = settingsSection.GetValue<int>("Id", 1);

if (settingsRepository.GetById(settingsId) is null)
{
    var settings = new TournamentSettings(
        settingsId,
        settingsSection.GetValue<DateTime>("StartDate", new DateTime(2026, 6, 1)),
        settingsSection.GetValue<int>("MaxMatchesPerDay", 3),
        settingsSection.GetValue<int>("DaysBetweenJourneys", 3),
        settingsSection.GetValue<int>("FixtureSeed", 0),
        settingsSection.GetValue<int>("CrossingsSeed", 0),
        settingsSection.GetValue<int>("SimulationSeed", 0),
        settingsSection.GetValue<int>("CompletionSeed", 0));

    settingsRepository.Add(settings);
}


// funcion implementada con IA para la correcta asignacion de roles en la base de datos
using (var scope = app.Services.CreateScope())
{
    var userServices = scope.ServiceProvider.GetRequiredService<UserServices>();

    var admin = userServices.GetAllUsers().FirstOrDefault(u => u.email == "admin@copa.com");

    if (admin == null)
    {
        admin = new User(
            0,
            "Admin",
            "Sistema",
            "admin@copa.com",
            "Admin@123",
            new DateTime(1990, 1, 1)
        );
        admin.AddRole(UserRoles.Admin);
        userServices.AddUser(admin);
    }
    else if (!admin.HasRole(UserRoles.Admin))
    {
        userServices.AssignRole(admin.idUser, UserRoles.Admin);
    }

    var editor = userServices.GetAllUsers().FirstOrDefault(u => u.email == "editor@copa.com");

    if (editor == null)
    {
        editor = new User(
            0,
            "Editor",
            "Sistema",
            "editor@copa.com",
            "Editor@123",
            new DateTime(1995, 5, 15)
        );
        editor.AddRole(UserRoles.Editor);
        userServices.AddUser(editor);
    }
    else if (!editor.HasRole(UserRoles.Editor))
    {
        userServices.AssignRole(editor.idUser, UserRoles.Editor);
    }

    var journalist = userServices.GetAllUsers().FirstOrDefault(u => u.email == "periodista@copa.com");

    if (journalist == null)
    {
        journalist = new User(
            0,
            "Periodista",
            "Sistema",
            "periodista@copa.com",
            "Periodista@123",
            new DateTime(1998, 3, 10)
        );
        journalist.AddRole(UserRoles.Periodista);
        userServices.AddUser(journalist);
    }
    else if (!journalist.HasRole(UserRoles.Periodista))
    {
        userServices.AssignRole(journalist.idUser, UserRoles.Periodista);
    }
}


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
