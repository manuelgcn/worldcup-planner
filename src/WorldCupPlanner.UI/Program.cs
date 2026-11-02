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
// Sin Cache-Control el navegador reusa por heurística el CSS de una versión anterior; "no-cache"
// lo obliga a revalidar con el ETag (un 304 barato) y cada entrega se ve con sus estilos.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache"
});
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


// Cuentas de demo: se crean solo si faltan y nunca se tocan las existentes. Antes se les reasignaba
// el rol en cada arranque, así que quien registrara uno de estos emails quedaba como Admin o Editor.
using (var scope = app.Services.CreateScope())
{
    var userServices = scope.ServiceProvider.GetRequiredService<IUserService>();
    HashSet<string> existingEmails = userServices.GetAllUsers()
        .Select(user => user.email)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    (string Name, string Email, string Password, DateTime BirthDate, UserRoles Role)[] demoAccounts =
    {
        ("Admin", "admin@copa.com", "Admin@123", new DateTime(1990, 1, 1), UserRoles.Admin),
        ("Editor", "editor@copa.com", "Editor@123", new DateTime(1995, 5, 15), UserRoles.Editor),
        ("Periodista", "periodista@copa.com", "Periodista@123", new DateTime(1998, 3, 10), UserRoles.Periodista)
    };

    foreach (var account in demoAccounts.Where(account => !existingEmails.Contains(account.Email)))
    {
        var user = new User(0, account.Name, "Sistema", account.Email, account.Password, account.BirthDate);
        user.AddRole(account.Role);
        userServices.AddUser(user);
    }
}


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
