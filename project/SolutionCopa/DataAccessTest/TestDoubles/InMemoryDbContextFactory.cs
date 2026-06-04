using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccessTest.TestDoubles;

// Test double de IDbContextFactory<AppDbContext> sobre el provider InMemory de EF Core,
// compartido por las suites de repositorios (Team, User, Stadium, TournamentSettings, AuditLog).
// Cada instancia usa un nombre de BD unico (Guid) para aislar los tests entre si, pero
// todos los contextos creados por la misma instancia comparten ese almacenamiento, de
// modo que se puede verificar la persistencia entre contextos efimeros (autosave).
public sealed class InMemoryDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public InMemoryDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"appdb-{Guid.NewGuid()}")
            .Options;
    }

    public AppDbContext CreateDbContext() => new AppDbContext(_options);
}
