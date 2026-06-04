using DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Stadium> Stadiums => Set<Stadium>();
    public DbSet<TournamentSettings> TournamentSettings => Set<TournamentSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new StadiumConfiguration());
        modelBuilder.ApplyConfiguration(new TournamentSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }
}
