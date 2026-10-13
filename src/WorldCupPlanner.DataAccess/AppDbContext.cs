using WorldCupPlanner.DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Stadium> Stadiums => Set<Stadium>();
    public DbSet<TournamentSettings> TournamentSettings => Set<TournamentSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchIncident> MatchIncidents => Set<MatchIncident>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new StadiumConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new JourneyConfiguration());
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
        modelBuilder.ApplyConfiguration(new MatchGroupConfiguration());
        modelBuilder.ApplyConfiguration(new MatchEliminationConfiguration());
        modelBuilder.ApplyConfiguration(new MatchIncidentConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}
