using Services;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using UI;
using UI.Components;
using ProjectCopa;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSingleton<ITeamRepository, TeamRepository>();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWorkRepository>();
builder.Services.AddSingleton<UserServices>();
builder.Services.AddSingleton<StadiumServices>();
builder.Services.AddSingleton<TeamServices>();
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
