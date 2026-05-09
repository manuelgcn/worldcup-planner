using ProjectCopa;
using Services;
using Services.DTOs;
using ServiceTest.TestDoubles.Mocks;

namespace ServiceTest;

[TestClass]
public class AuditLogServiceTests
{
    private MockAuditLogRepository _repository = null!;
    private AuditLogService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new MockAuditLogRepository();
        _service = AuditLogService.CreateInstance(_repository);
    }

    [TestMethod]
    public void Log_WithValidData_CallsRepositoryAdd()
    {
        // Act
        _service.Log("CREATE", "Match Created", 1, "Match", 10);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("CREATE", logs[0].Action);
        Assert.AreEqual("Match Created", logs[0].Details);
        Assert.AreEqual(1, logs[0].TournamentSettingsId);
        Assert.AreEqual("Match", logs[0].AffectedEntityType);
        Assert.AreEqual(10, logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void Log_WithOptionalEntityData_CallsRepositoryAddWithCorrectNulls()
    {
        // Act
        _service.Log("EXPORT", "Data exported", 2);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("EXPORT", logs[0].Action);
        Assert.AreEqual("Data exported", logs[0].Details);
        Assert.AreEqual(2, logs[0].TournamentSettingsId);
        Assert.IsNull(logs[0].AffectedEntityType);
        Assert.IsNull(logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtos()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("ACTION1", result[0].Action);
        Assert.AreEqual("ACTION2", result[1].Action);
        Assert.AreEqual(1, result[0].TournamentSettingsId);
        Assert.AreEqual(2, result[1].TournamentSettingsId);
    }

    [TestMethod]
    public void GetByTournamentSettings_ReturnsFilteredDtos()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2); // Debería excluirse
        AuditLog log3 = AuditLog.Create("ACTION3", "Det 3", 1);
        _repository.Seed(log1);
        _repository.Seed(log2);
        _repository.Seed(log3);

        // Act
        List<AuditLogDto> result = _service.GetByTournamentSettings(1);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("ACTION1", result[0].Action);
        Assert.AreEqual("ACTION3", result[1].Action);
    }

    [TestMethod]
    public void ExportAsText_WithNullTournamentId_ReturnsAllLogsText()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("ACTION1"));
        Assert.IsTrue(result.Contains("ACTION2"));
        Assert.IsTrue(result.Contains("TournamentID: 1"));
        Assert.IsTrue(result.Contains("TournamentID: 2"));
    }

    [TestMethod]
    public void ExportAsText_WithSpecificTournamentId_ReturnsFilteredText()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2); // Debería excluirse del texto
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        string result = _service.ExportAsText(1);

        // Assert
        Assert.IsTrue(result.Contains("ACTION1"));
        Assert.IsFalse(result.Contains("ACTION2"));
        Assert.IsFalse(result.Contains("TournamentID: 2"));
    }

    [TestMethod]
    public void ExportAsText_WithEntities_FormatsTextCorrectly()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("UPDATE", "Settings updated", 1, "TournamentSettings", 1);
        _repository.Seed(log1);

        // Act
        string result = _service.ExportAsText(1);

        // Assert
        Assert.IsTrue(result.Contains("Entity: TournamentSettings - ID: 1"));
        Assert.IsTrue(result.Contains("Action: UPDATE"));
    }

    [TestMethod]
    public void ExportAsText_WithoutEntities_FormatsTextCorrectly()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("EXPORT", "Full Export", 1);
        _repository.Seed(log1);

        // Act
        string result = _service.ExportAsText(1);

        // Assert
        Assert.IsFalse(result.Contains("Entity:"));
        Assert.IsTrue(result.Contains("Action: EXPORT"));
    }
}