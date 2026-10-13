using WorldCupPlanner.Services;
using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.UI;
using WorldCupPlanner.UI.Components;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string");

string flagsDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration.GetValue<string>("Flags:Directory") ?? "flags-master");

builder.Services.AddWorldCupPlanner(connectionString, flagsDirectory);
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

// Las rutas inexistentes muestran la página 404 de la app en vez de una respuesta vacía.
app.UseStatusCodePagesWithReExecute("/not-found");

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
    var userServices = scope.ServiceProvider.GetRequiredService<IUserService>();

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
