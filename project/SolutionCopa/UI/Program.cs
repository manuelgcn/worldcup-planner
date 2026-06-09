using Services;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using UI;
using UI.Components;
using ProjectCopa;
using Services.Interfaces;
using Services.Parsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<ITeamRepository, TeamRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

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

builder.Services.AddSingleton<IMatchSimulationEngine, ProbabilisticSimulationEngine>();
builder.Services.AddSingleton<IAuditLogService>(sp =>
    AuditLogService.CreateInstance(
        sp.GetRequiredService<IAuditLogRepository>(),
        1));

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

builder.Services.AddSingleton<IKnockoutFixtureGenerator>(sp =>
    sp.GetRequiredService<KnockoutFixtureGenerator>());

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
}


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
