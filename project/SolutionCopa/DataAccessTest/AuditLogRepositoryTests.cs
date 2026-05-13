using DataAccess.Repositories;
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
        _repository = new AuditLogRepository();
    }

    [TestMethod]
    public void AddStoresLogAndGetAllReturnsIt()
    {
        AuditLog log = AuditLog.Create("CREATE", "Created match", 1, "Match", 10);

        _repository.Add(log);

        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE", logs[0].Action);
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
}
