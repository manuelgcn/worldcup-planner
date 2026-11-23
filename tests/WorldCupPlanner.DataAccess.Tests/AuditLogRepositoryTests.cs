using WorldCupPlanner.DataAccess.Repositories;
using WorldCupPlanner.DataAccess.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services;

namespace WorldCupPlanner.DataAccess.Tests;

[TestClass]
public class AuditLogRepositoryTests
{
    private AuditLogRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        // Contextos efimeros sobre una BD InMemory aislada por test (mismo patron que TeamRepositoryTests).
        _repository = new AuditLogRepository(new InMemoryDbContextFactory());
    }

    [TestMethod]
    public void AddStoresLogAndGetAllReturnsIt()
    {
        AuditLog log = AuditLog.Create("CREATE", "Created match", 1, "Match", 10);

        _repository.Add(log);                       // contexto nuevo en GetAll: el dato viene de la "BD"

        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE", logs[0].Action);
        Assert.AreEqual("Created match", logs[0].Details);
        Assert.AreEqual("Match", logs[0].AffectedEntityType);
        Assert.AreEqual(10, logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void AddAssignsIdentityIdOnTheEntity()
    {
        AuditLog log = AuditLog.Create("CREATE", "Created match", 1);

        _repository.Add(log);

        Assert.IsTrue(log.Id > 0);                  // la BD asigna el Id (IDENTITY) y EF lo refleja en la entidad
    }

    [TestMethod]
    public void GetAllReturnsLogsInInsertionOrder()
    {
        _repository.Add(AuditLog.Create("FIRST", "first", 1));
        _repository.Add(AuditLog.Create("SECOND", "second", 1));

        List<AuditLog> logs = _repository.GetAll();

        Assert.AreEqual(2, logs.Count);
        Assert.AreEqual("FIRST", logs[0].Action);
        Assert.AreEqual("SECOND", logs[1].Action);
    }

    [TestMethod]
    public void GetAllReturnsCopyOfStoredLogs()
    {
        AuditLog log = AuditLog.Create("CREATE", "Created match", 1);
        _repository.Add(log);

        List<AuditLog> logs = _repository.GetAll();
        logs.Clear();

        Assert.AreEqual(1, _repository.GetAll().Count);
    }

    [TestMethod]
    public void AddPersistsGlobalLogWithoutTournament()
    {
        _repository.Add(AuditLog.Create("GLOBAL", "Global log"));

        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.IsNull(logs[0].TournamentSettingsId);
    }

    [TestMethod]
    public void AddPersistsPerformedByEmail()
    {
        _repository.Add(AuditLog.Create("GLOBAL", "Global log", performedByEmail: "admin@copa.com"));

        Assert.AreEqual("admin@copa.com", _repository.GetAll()[0].PerformedByEmail);
    }

    // Sobre SQLite, que impone las claves foraneas: el id se guarda aunque ese usuario no exista (o ya se haya borrado).
    [TestMethod]
    public void AddPersistsPerformedByUserIdEvenWhenNoSuchUserExists()
    {
        using SqliteDbContextFactory factory = new SqliteDbContextFactory();
        AuditLogRepository repository = new AuditLogRepository(factory);

        repository.Add(AuditLog.Create("GLOBAL", "Global log", performedByEmail: "admin@copa.com", performedByUserId: 4242));
        repository.Add(AuditLog.Create("GLOBAL", "No actor"));

        List<AuditLog> logs = repository.GetAll();
        Assert.AreEqual(4242, logs[0].PerformedByUserId);
        Assert.IsNull(logs[1].PerformedByUserId);
    }

    // El caso que motivo guardar el id: el actor cambia su email, o se lo borra, y sus registros lo siguen identificando.
    [TestMethod]
    public void LogsKeepTheActorIdAfterTheUserChangesEmailAndIsDeleted()
    {
        using SqliteDbContextFactory factory = new SqliteDbContextFactory();
        UserRepository users = new UserRepository(factory);
        AuditLogRepository logs = new AuditLogRepository(factory);
        AuditLogService service = AuditLogService.CreateInstance(logs, users);
        User actor = new User(0, "Ana", "Admin", "ana@copa.com", "Pass123@", new DateTime(1990, 5, 15));
        actor.AddRole(UserRoles.Admin);
        users.Add(actor);
        int actorId = actor.idUser;

        service.LogPerformedBy("ACTION", "Before the change", "ana@copa.com");
        actor.UpdatePersonalData("Ana", "Admin", "otra@copa.com");
        users.Update(actor);
        service.LogPerformedBy("ACTION", "After the change", "otra@copa.com");
        users.Remove(actor);

        List<AuditLog> stored = logs.GetAll();
        Assert.AreEqual(2, stored.Count);
        Assert.IsTrue(actorId > 0);
        Assert.IsTrue(stored.All(log => log.PerformedByUserId == actorId));
    }

    // Por el caso de uso real: al loguear, el email con el que el actor entro ya no es de nadie.
    [TestMethod]
    public void UpdatingOnesOwnEmailThroughUserServicesKeepsTheActorId()
    {
        using SqliteDbContextFactory factory = new SqliteDbContextFactory();
        UserRepository users = new UserRepository(factory);
        AuditLogRepository logs = new AuditLogRepository(factory);
        UserServices userServices = new UserServices(users, AuditLogService.CreateInstance(logs, users));
        // El id 1 es el administrador del sistema, que no se puede modificar: el actor va segundo.
        users.Add(new User(0, "Admin", "Sistema", "admin@copa.com", "Pass123@", new DateTime(1990, 5, 15)));
        User actor = new User(0, "Ana", "Editora", "ana@copa.com", "Pass123@", new DateTime(1990, 5, 15));
        actor.AddRole(UserRoles.Editor);
        users.Add(actor);

        userServices.UpdateUser(actor.idUser, "Ana", "Editora", "otra@copa.com", performedByEmail: "ana@copa.com");

        AuditLog log = logs.GetAll().Single(l => l.Action == AuditActions.UpdateUser);
        Assert.AreEqual(actor.idUser, log.PerformedByUserId);
        Assert.AreEqual("otra@copa.com", log.PerformedByEmail);
    }

    [TestMethod]
    public void AddKeepsMaterializingLogsWithTournamentContext()
    {
        _repository.Add(AuditLog.Create("LEGACY", "With tournament", 7));

        Assert.AreEqual(7, _repository.GetAll()[0].TournamentSettingsId);
    }

    [TestMethod]
    public void GetFilteredWithoutFiltersReturnsAllLogs()
    {
        _repository.Add(AuditLog.Create("FIRST", "first"));
        _repository.Add(AuditLog.Create("SECOND", "second"));

        List<AuditLog> result = _repository.GetFiltered(null, null, null, null);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetFilteredAppliesInclusiveDateRange()
    {
        AuditLog log = AuditLog.Create("TODAY", "Created today");
        _repository.Add(log);

        List<AuditLog> inside = _repository.GetFiltered(log.Timestamp, log.Timestamp, null, null);
        List<AuditLog> outside = _repository.GetFiltered(log.Timestamp.AddSeconds(1), null, null, null);

        Assert.AreEqual(1, inside.Count);
        Assert.AreEqual(0, outside.Count);
    }

    [TestMethod]
    public void GetFilteredMatchesActionPartialAndCaseInsensitive()
    {
        _repository.Add(AuditLog.Create("CREATE_USER", "user created"));
        _repository.Add(AuditLog.Create("DELETE_TEAM", "team deleted"));

        List<AuditLog> result = _repository.GetFiltered(null, null, "create_u", null);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CREATE_USER", result[0].Action);
    }

    [TestMethod]
    public void GetFilteredMatchesActorPartialAndCaseInsensitive()
    {
        _repository.Add(AuditLog.Create("ACTION", "by admin", performedByEmail: "Admin@copa.com"));
        _repository.Add(AuditLog.Create("ACTION", "by editor", performedByEmail: "editor@copa.com"));
        _repository.Add(AuditLog.Create("ACTION", "anonymous"));

        List<AuditLog> result = _repository.GetFiltered(null, null, null, "ADMIN");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Admin@copa.com", result[0].PerformedByEmail);
    }

    [TestMethod]
    public void GetFilteredCombinesAllFilters()
    {
        AuditLog match = AuditLog.Create("CREATE_USER", "match", performedByEmail: "admin@copa.com");
        _repository.Add(match);
        _repository.Add(AuditLog.Create("CREATE_USER", "wrong actor", performedByEmail: "editor@copa.com"));
        _repository.Add(AuditLog.Create("DELETE_TEAM", "wrong action", performedByEmail: "admin@copa.com"));

        List<AuditLog> result = _repository.GetFiltered(
            match.Timestamp.AddSeconds(-1),
            match.Timestamp.AddSeconds(1),
            "CREATE",
            "admin");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("match", result[0].Details);
    }

    [TestMethod]
    public void GetFilteredOrdersByTimestampThenId()
    {
        _repository.Add(AuditLog.Create("FIRST", "first"));
        _repository.Add(AuditLog.Create("SECOND", "second"));

        List<AuditLog> result = _repository.GetFiltered(null, null, null, null);

        Assert.AreEqual("FIRST", result[0].Action);
        Assert.AreEqual("SECOND", result[1].Action);
    }

    [TestMethod]
    public void GetFiltered_WithTake_ReturnsNewestFirst()
    {
        _repository.Add(AuditLog.Create("FIRST", "first"));
        _repository.Add(AuditLog.Create("SECOND", "second"));
        _repository.Add(AuditLog.Create("THIRD", "third"));

        List<AuditLog> result = _repository.GetFiltered(null, null, null, null, take: 2);

        CollectionAssert.AreEqual(new[] { "THIRD", "SECOND" }, result.Select(log => log.Action).ToArray());
    }
}
