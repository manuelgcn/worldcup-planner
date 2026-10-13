using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorldCupPlanner.DataAccess;

// Permite a `dotnet ef` crear el AppDbContext sin levantar la UI (generacion de migraciones).
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string FallbackConnectionString =
        "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=WorldCup2026!Dev;TrustServerCertificate=True;";

    public AppDbContext CreateDbContext(string[] args)
    {
        string connection = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? FallbackConnectionString;

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new AppDbContext(options);
    }
}
