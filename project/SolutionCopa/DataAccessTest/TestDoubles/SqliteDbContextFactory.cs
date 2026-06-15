using DataAccess;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccessTest.TestDoubles;

// Test double de IDbContextFactory<AppDbContext> sobre SQLite en memoria.
//
// A diferencia del provider InMemory (que NO es relacional y NO valida foreign keys), SQLite es
// una base relacional real: respeta las FK y la jerarquia TPH. Por eso se usa en los tests de
// integracion de persistencia, donde el objetivo es justamente que una FK invalida falle como
// fallaria contra SQL Server -algo que InMemory ocultaba-.
//
// La BD ":memory:" vive mientras la conexion este ABIERTA; por eso se mantiene una unica conexion
// compartida y abierta durante toda la vida del factory. Cada CreateDbContext() abre un contexto
// efimero sobre esa misma conexion (igual que en produccion: contexto por operacion, autosave),
// de modo que la persistencia se ve entre contextos. "Foreign Keys=True" garantiza que la
// imposicion de FK quede activa aun cuando EF reutiliza una conexion ya abierta por nosotros.
public sealed class SqliteDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using AppDbContext context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new AppDbContext(_options);

    public void Dispose() => _connection.Dispose();
}
