using DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;
using ProjectCopa;

namespace DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<User> Users => Set<User>();
    
    public DbSet<UserRole> UserRoles { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        base.OnModelCreating(modelBuilder);
        
        
        modelBuilder.Entity<UserRole>()
            .ToTable("UserRoles")
            .HasOne(ur => ur.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
    
}
