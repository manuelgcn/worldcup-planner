using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Parsers;

namespace WorldCupPlanner.DataSeeder;

// Seeder externo al codigo productivo (Fase 11 del Obligatorio 2).
// Reutiliza los Services reales para garantizar datos validos por dominio:
// usuarios, estadios, 48 equipos con bandera, fixture de grupos generado y
// simulado, mas las entradas de auditoria que generan esas operaciones.
//
// Uso:
//   dotnet run --project DataSeeder -- "<connection string>"
// Si no se pasa cadena, usa la de desarrollo (SQL Edge en localhost,1433).
public static class Program
{
    private const string DefaultConnectionString =
        "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=WorldCup2026!Dev;TrustServerCertificate=True;";

    private const int SettingsId = 1;
    private const int SimulationSeed = 2026;
    private const int FixtureSeed = 7;

    public static int Main(string[] args)
    {
        string connectionString = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
            ? args[0]
            : DefaultConnectionString;

        Console.WriteLine("== WorldCupPlanner 2026 — DataSeeder ==");
        Console.WriteLine($"Conexion: {Mask(connectionString)}");

        ServiceProvider provider = BuildServiceProvider(connectionString);

        ResetDatabase(provider);
        SeedTournamentSettings(provider);
        SeedUsers(provider);
        SeedStadiums(provider);
        SeedTeamsWithFlags(provider);
        GenerateAndSimulateGroupStage(provider);

        PrintSummary(provider);
        Console.WriteLine("Seed completo. La base quedo lista para usar end-to-end.");
        return 0;
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<ITeamRepository, TeamRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IMatchRepository, MatchRepository>();
        services.AddSingleton<IStadiumRepository, StadiumRepository>();
        services.AddSingleton<IGroupRepository, GroupRepository>();
        services.AddSingleton<IJourneyRepository, JourneyRepository>();
        services.AddSingleton<ITournamentSettingsRepository, TournamentSettingsRepository>();
        services.AddSingleton<IEliminationMatchRepository, EliminationMatchRepository>();
        services.AddSingleton<IAuditLogRepository, AuditLogRepository>();

        services.AddSingleton<UserServices>();
        services.AddSingleton<IUserService>(sp => sp.GetRequiredService<UserServices>());
        services.AddSingleton<TeamServices>();
        services.AddSingleton<ITeamService>(sp => sp.GetRequiredService<TeamServices>());
        services.AddSingleton<StadiumServices>();
        services.AddSingleton<IStadiumService>(sp => sp.GetRequiredService<StadiumServices>());
        services.AddSingleton<GroupServices>();
        services.AddSingleton<IGroupService>(sp => sp.GetRequiredService<GroupServices>());
        services.AddSingleton<JourneyServices>();
        services.AddSingleton<IJourneyService>(sp => sp.GetRequiredService<JourneyServices>());
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddSingleton<IMatchSimulationEngine, ProbabilisticSimulationEngine>();
        services.AddSingleton<IMatchSimulationEngine, PureRandomSimulationEngine>();
        services.AddSingleton<ISimulationEngineRegistry, SimulationEngineRegistry>();
        services.AddSingleton<IAuditLogService>(sp =>
            AuditLogService.CreateInstance(sp.GetRequiredService<IAuditLogRepository>()));

        services.AddSingleton<IRankingService, RankingService>();
        services.AddSingleton<IMatchFactory, MatchFactory>();
        services.AddSingleton<IMatchService, MatchService>();

        services.AddSingleton<KnockoutFixtureGenerator>(sp =>
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
        services.AddSingleton<IKnockoutFixtureGenerator>(sp =>
            sp.GetRequiredService<KnockoutFixtureGenerator>());

        services.AddSingleton<ITournamentSimulationService, TournamentSimulationService>();
        services.AddSingleton<IFixtureOrchestrator, FixtureOrchestrator>();
        services.AddSingleton<ITeamParser, CsvStreamTeamParser>();

        services.AddSingleton<IImportService>(sp =>
            ImportService.CreateInstance(
                sp.GetRequiredService<ITeamRepository>(),
                sp.GetRequiredService<ITeamParser>()));

        string flagsDirectory = Path.Combine(AppContext.BaseDirectory, "flags-master");
        services.AddSingleton<IFlagAssignmentService>(sp =>
            new FlagAssignmentService(
                sp.GetRequiredService<ITeamRepository>(),
                flagsDirectory,
                sp.GetRequiredService<IAuditLogService>()));

        return services.BuildServiceProvider();
    }

    // Estado limpio y reproducible: borra el esquema y vuelve a aplicar migraciones.
    private static void ResetDatabase(ServiceProvider provider)
    {
        Console.WriteLine("Reseteando esquema (EnsureDeleted + Migrate)...");
        using AppDbContext context = provider
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }

    private static void SeedTournamentSettings(ServiceProvider provider)
    {
        var repository = provider.GetRequiredService<ITournamentSettingsRepository>();
        if (repository.GetById(SettingsId) is not null)
        {
            return;
        }

        var settings = new TournamentSettings(
            SettingsId,
            new DateTime(2026, 6, 1),
            maxMatchesPerDay: 3,
            daysBetweenJourneys: 3,
            fixtureSeed: FixtureSeed,
            crossingsSeed: 0,
            simulationSeed: SimulationSeed,
            completionSeed: 0);
        repository.Add(settings);
        Console.WriteLine("- TournamentSettings sembrado (Id=1).");
    }

    // 2 administradores, 2 editores, 2 periodistas (passwords cumplen la politica).
    private static void SeedUsers(ServiceProvider provider)
    {
        var userServices = provider.GetRequiredService<UserServices>();

        (string Name, string Last, string Email, string Password, DateTime Birth, UserRoles Role)[] seeds =
        {
            ("Ana", "Admin", "admin1@copa.com", "Admin@123", new DateTime(1988, 2, 10), UserRoles.Admin),
            ("Bruno", "Admin", "admin2@copa.com", "Admin@456", new DateTime(1990, 7, 22), UserRoles.Admin),
            ("Carla", "Editor", "editor1@copa.com", "Editor@123", new DateTime(1992, 4, 5), UserRoles.Editor),
            ("Diego", "Editor", "editor2@copa.com", "Editor@456", new DateTime(1993, 11, 30), UserRoles.Editor),
            ("Elena", "Prensa", "periodista1@copa.com", "Prensa@123", new DateTime(1995, 6, 18), UserRoles.Periodista),
            ("Felipe", "Prensa", "periodista2@copa.com", "Prensa@456", new DateTime(1996, 9, 3), UserRoles.Periodista),
        };

        foreach (var seed in seeds)
        {
            var user = new User(0, seed.Name, seed.Last, seed.Email, seed.Password, seed.Birth);
            user.AddRole(seed.Role);
            userServices.AddUser(user);
        }

        Console.WriteLine($"- {seeds.Length} usuarios sembrados (2 admin / 2 editor / 2 periodista).");
    }

    // 5 estadios (uno mas que el minimo de 4 exigido por el fixture).
    private static void SeedStadiums(ServiceProvider provider)
    {
        var stadiumServices = provider.GetRequiredService<StadiumServices>();

        (string Name, string City, string? Description, int Capacity)[] stadiums =
        {
            ("MetLife Stadium", "East Rutherford", "Sede de la final.", 82500),
            ("AT&T Stadium", "Arlington", null, 80000),
            ("SoFi Stadium", "Inglewood", null, 70000),
            ("Estadio Azteca", "Ciudad de Mexico", "Estadio historico.", 87000),
            ("BMO Field", "Toronto", null, 30000),
        };

        int id = 1;
        foreach (var stadium in stadiums)
        {
            stadiumServices.AddStadium(new Stadium(stadium.Name, stadium.City, stadium.Description, stadium.Capacity) { Id = id });
            id++;
        }

        Console.WriteLine($"- {stadiums.Length} estadios sembrados.");
    }

    // 48 equipos reales (CSV oficial) + asignacion de banderas (las que tienen mapeo ISO).
    private static void SeedTeamsWithFlags(ServiceProvider provider)
    {
        var importService = provider.GetRequiredService<IImportService>();
        var flagService = provider.GetRequiredService<IFlagAssignmentService>();

        string csvPath = Path.Combine(AppContext.BaseDirectory, "data", "teams-wc2026-48.csv");
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"No se encontro el CSV de equipos en {csvPath}.");
        }

        using (FileStream stream = File.OpenRead(csvPath))
        {
            var result = importService.ImportTeams(stream);
            Console.WriteLine($"- Equipos importados: {result.ImportedCount}.");
        }

        var flagResult = flagService.AssignFlags(overwriteExisting: true, performedByEmail: "admin1@copa.com");
        Console.WriteLine($"- Banderas asignadas: {flagResult.AssignedCount} (sin mapeo: {flagResult.UnmappedTeamNames.Count}).");
    }

    private static void GenerateAndSimulateGroupStage(ServiceProvider provider)
    {
        var orchestrator = provider.GetRequiredService<IFixtureOrchestrator>();
        var simulation = provider.GetRequiredService<ITournamentSimulationService>();

        orchestrator.GenerateGroupStage(
            fixtureSeedOverride: FixtureSeed,
            selectedEngineName: "Probabilístico",
            performedByEmail: "admin1@copa.com");
        Console.WriteLine("- Fixture de grupos generado (motor Probabilístico).");

        simulation.SimulateGroupStage(SimulationSeed, performedByEmail: "editor1@copa.com");
        Console.WriteLine("- Fase de grupos simulada (recalcula ranking, audita y notifica).");
    }

    private static void PrintSummary(ServiceProvider provider)
    {
        using AppDbContext context = provider
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContext();

        Console.WriteLine("== Resumen ==");
        Console.WriteLine($"  Usuarios:       {context.Users.Count()}");
        Console.WriteLine($"  Estadios:       {context.Stadiums.Count()}");
        Console.WriteLine($"  Equipos:        {context.Teams.Count()} (con bandera: {context.Teams.Count(t => t.FlagBase64 != null)})");
        Console.WriteLine($"  Grupos:         {context.Groups.Count()}");
        Console.WriteLine($"  Partidos:       {context.Matches.Count()}");
        Console.WriteLine($"  Notificaciones: {context.Notifications.Count()}");
        Console.WriteLine($"  Audit logs:     {context.AuditLogs.Count()}");
    }

    private static string Mask(string connectionString)
    {
        int idx = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return connectionString;
        }

        int end = connectionString.IndexOf(';', idx);
        string tail = end < 0 ? string.Empty : connectionString[end..];
        return connectionString[..idx] + "Password=***" + tail;
    }
}
