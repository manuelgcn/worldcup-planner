using Services;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using UI;
using UI.Components;
using ProjectCopa;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSingleton<ITeamRepository, TeamRepositoryList>();
builder.Services.AddSingleton<IMatchRepository, MatchRepository>();
builder.Services.AddSingleton<IStadiumRepository, StadiumRepository>();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWorkRepository>();



builder.Services.AddSingleton<UserServices>();
builder.Services.AddSingleton<StadiumServices>();
builder.Services.AddSingleton<TeamServices>();


builder.Services.AddScoped<SessionState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

var userServices = app.Services.GetRequiredService<UserServices>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();