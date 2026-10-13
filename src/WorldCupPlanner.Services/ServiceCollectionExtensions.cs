using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldCupPlanner.DataAccess;
using WorldCupPlanner.DataAccess.Interfaces;
using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Parsers;

namespace WorldCupPlanner.Services;

// Unico registro de repositorios y servicios: la UI y el DataSeeder arman el mismo grafo.
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorldCupPlanner(this IServiceCollection services, string connectionString, string flagsDirectory)
    {
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

        services.AddSingleton<IAuditLogService>(sp =>
            AuditLogService.CreateInstance(sp.GetRequiredService<IAuditLogRepository>()));
        services.AddSingleton<IUserService, UserServices>();
        services.AddSingleton<ITeamService, TeamServices>();
        services.AddSingleton<IStadiumService, StadiumServices>();
        services.AddSingleton<IGroupService, GroupServices>();
        services.AddSingleton<IJourneyService, JourneyServices>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IExporter, Exporter>();
        services.AddSingleton<IExportService, ExportService>();

        services.AddSingleton<IMatchSimulationEngine, ProbabilisticSimulationEngine>();
        services.AddSingleton<IMatchSimulationEngine, PureRandomSimulationEngine>(); // OCP: nuevos motores se agregan aca sin tocar consumidores
        services.AddSingleton<ISimulationEngineRegistry, SimulationEngineRegistry>();

        services.AddSingleton<IRankingService, RankingService>();
        services.AddSingleton<IMatchFactory, MatchFactory>();
        services.AddSingleton<IMatchService, MatchService>();
        services.AddSingleton<IMatchQueryService, MatchQueryService>();
        services.AddSingleton<ITournamentDashboardService, TournamentDashboardService>();
        services.AddSingleton<ISimulationConfigurationService, SimulationConfigurationService>();
        services.AddSingleton<IFixtureViewService, FixtureViewService>();
        services.AddSingleton<IKnockoutFixtureGenerator, KnockoutFixtureGenerator>();
        services.AddSingleton<ITournamentSimulationService, TournamentSimulationService>();
        services.AddSingleton<IFixtureOrchestrator, FixtureOrchestrator>();
        services.AddSingleton<IGroupStandingsService, GroupStandingsService>();

        services.AddSingleton<ITeamParser, CsvStreamTeamParser>();
        services.AddSingleton<IImportService>(sp =>
            ImportService.CreateInstance(
                sp.GetRequiredService<ITeamRepository>(),
                sp.GetRequiredService<ITeamParser>(),
                sp.GetRequiredService<IAuditLogService>()));
        services.AddSingleton<IFlagAssignmentService>(sp =>
            new FlagAssignmentService(
                sp.GetRequiredService<ITeamRepository>(),
                flagsDirectory,
                sp.GetRequiredService<IAuditLogService>()));

        return services;
    }
}
