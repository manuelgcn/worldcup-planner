using DataAccess.Repositories;
using DataAccessTest.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectCopa;

namespace DataAccessTest;

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
    public void GetByDateRangeReturnsOnlyLogsInsideInclusiveRange()
    {
        AuditLog log = AuditLog.Create("TODAY", "Created today", 1);
        _repository.Add(log);

        List<AuditLog> result = _repository.GetByDateRange(
            log.Timestamp.AddSeconds(-1),
            log.Timestamp.AddSeconds(1));

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("TODAY", result[0].Action);
    }
}
