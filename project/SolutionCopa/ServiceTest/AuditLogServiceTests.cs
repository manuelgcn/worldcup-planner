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
        _service = AuditLogService.CreateInstance(_repository, 1);
    }

    [TestMethod]
    public void CreateInstance_ReturnsServiceThatCanLog()
    {
        // Act
        _service.Log("CREATE", "Match Created", 1, "Match", 10);

        // Assert
        Assert.AreEqual(1, _repository.GetAll().Count);
        Assert.AreEqual("CREATE", _repository.GetAll()[0].Action);
    }

    [TestMethod]
    public void Log_WithValidData_StoresDetailsAndTournamentSettings()
    {
        // Act
        _service.Log("CREATE", "Match Created", 1, "Match", 10);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual("Match Created", logs[0].Details);
        Assert.AreEqual(1, logs[0].TournamentSettingsId);
    }

    [TestMethod]
    public void Log_WithValidData_StoresEntityData()
    {
        // Act
        _service.Log("CREATE", "Match Created", 1, "Match", 10);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual("Match", logs[0].AffectedEntityType);
        Assert.AreEqual(10, logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void Log_WithOptionalEntityData_StoresActionAndCount()
    {
        // Act
        _service.Log("EXPORT", "Data exported", 2);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("EXPORT", logs[0].Action);
    }

    [TestMethod]
    public void Log_WithOptionalEntityData_StoresNullEntityData()
    {
        // Act
        _service.Log("EXPORT", "Data exported", 2);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.IsNull(logs[0].AffectedEntityType);
        Assert.IsNull(logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void Log_WithoutTournamentSettingsId_UsesDefaultContext()
    {
        // Act
        _service.Log("EXPORT", "Data exported", "Export", 7);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual(1, logs[0].TournamentSettingsId);
    }

    [TestMethod]
    public void Log_WithoutTournamentSettingsId_StoresEntityData()
    {
        // Act
        _service.Log("EXPORT", "Data exported", "Export", 7);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual("Export", logs[0].AffectedEntityType);
        Assert.AreEqual(7, logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtosWithCorrectCount()
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
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtosWithCorrectActions()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        CollectionAssert.AreEqual(new List<string> { "ACTION1", "ACTION2" }, result.Select(dto => dto.Action).ToList());
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtosWithCorrectTournamentSettings()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        CollectionAssert.AreEqual(new List<int> { 1, 2 }, result.Select(dto => dto.TournamentSettingsId).ToList());
    }

    [TestMethod]
    public void GetByTournamentSettings_ReturnsFilteredDtos()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        AuditLog log3 = AuditLog.Create("ACTION3", "Det 3", 1);
        _repository.Seed(log1);
        _repository.Seed(log2);
        _repository.Seed(log3);

        // Act
        List<AuditLogDto> result = _service.GetByTournamentSettings(1);

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEqual(new List<string> { "ACTION1", "ACTION3" }, result.Select(dto => dto.Action).ToList());
    }

    [TestMethod]
    public void GetByTournamentSettings_ReturnsEmptyListWhenNoLogsMatch()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        List<AuditLogDto> result = _service.GetByTournamentSettings(99);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetByTournamentSettings_ReturnsEmptyListWhenRepositoryIsEmpty()
    {
        // Act
        List<AuditLogDto> result = _service.GetByTournamentSettings(1);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ExportAsText_WithNullTournamentId_ReturnsAllLogsActions()
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
    }

    [TestMethod]
    public void ExportAsText_WithNullTournamentId_ReturnsAllTournamentIds()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("TournamentID: 1"));
        Assert.IsTrue(result.Contains("TournamentID: 2"));
    }

    [TestMethod]
    public void ExportAsText_WithSpecificTournamentId_ReturnsFilteredActions()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        string result = _service.ExportAsText(1);

        // Assert
        Assert.IsTrue(result.Contains("ACTION1"));
        Assert.IsFalse(result.Contains("ACTION2"));
    }

    [TestMethod]
    public void ExportAsText_WithSpecificTournamentId_ExcludesOtherTournamentIds()
    {
        // Arrange
        AuditLog log1 = AuditLog.Create("ACTION1", "Det 1", 1);
        AuditLog log2 = AuditLog.Create("ACTION2", "Det 2", 2);
        _repository.Seed(log1);
        _repository.Seed(log2);

        // Act
        string result = _service.ExportAsText(1);

        // Assert
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

    [TestMethod]
    public void AuditActions_DefinesGenerateKnockoutConstant()
    {
        Assert.AreEqual("GENERATE_KNOCKOUT", AuditActions.GenerateKnockout);
    }

    [TestMethod]
    public void AuditActions_DefinesRegenerateFixtureConstant()
    {
        Assert.AreEqual("REGENERATE_FIXTURE", AuditActions.RegenerateFixture);
    }

    [TestMethod]
    public void AuditActions_DefinesClearFixtureConstant()
    {
        Assert.AreEqual("CLEAR_FIXTURE", AuditActions.ClearFixture);
    }

    [TestMethod]
    public void AuditActions_DefinesSimulateRoundConstant()
    {
        Assert.AreEqual("SIMULATE_ROUND", AuditActions.SimulateRound);
    }

    [TestMethod]
    public void LogPerformedBy_WithEmail_AppendsByEmailToDetails()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: "admin@copa.com",
            affectedEntityType: "Fixture");

        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("Fixture generated by admin@copa.com", logs[0].Details);
    }

    [TestMethod]
    public void LogPerformedBy_WithEmail_KeepsActionAndEntity()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateKnockout,
            "R32 generated",
            performedByEmail: "admin@copa.com",
            affectedEntityType: "Knockout",
            affectedEntityId: 42);

        AuditLog log = _repository.GetAll()[0];
        Assert.AreEqual(AuditActions.GenerateKnockout, log.Action);
        Assert.AreEqual("Knockout", log.AffectedEntityType);
        Assert.AreEqual(42, log.AffectedEntityId);
    }

    [TestMethod]
    public void LogPerformedBy_WithNullEmail_KeepsDetailsUnchanged()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: null);

        Assert.AreEqual("Fixture generated", _repository.GetAll()[0].Details);
    }

    [TestMethod]
    public void LogPerformedBy_WithWhitespaceEmail_KeepsDetailsUnchanged()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: "   ");

        Assert.AreEqual("Fixture generated", _repository.GetAll()[0].Details);
    }

    [TestMethod]
    public void LogPerformedBy_UsesDefaultTournamentSettingsId()
    {
        _service.LogPerformedBy(
            AuditActions.SimulateRound,
            "Round simulated",
            performedByEmail: "admin@copa.com");

        Assert.AreEqual(1, _repository.GetAll()[0].TournamentSettingsId);
    }

    [TestMethod]
    public void GetAll_PreservesInsertionOrder()
    {
        _service.Log("FIRST", "first action", 1);
        _service.Log("SECOND", "second action", 1);
        _service.Log("THIRD", "third action", 1);

        List<AuditLogDto> result = _service.GetAll();

        CollectionAssert.AreEqual(
            new List<string> { "FIRST", "SECOND", "THIRD" },
            result.Select(dto => dto.Action).ToList());
    }

    [TestMethod]
    public void Log_StoresIso8601RoundTrippableTimestamp()
    {
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        _service.Log("ACTION", "Details", 1);

        AuditLog log = _repository.GetAll()[0];
        string roundTripped = log.Timestamp.ToString("o");
        DateTime parsed = DateTime.Parse(
            roundTripped,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);

        Assert.AreEqual(log.Timestamp, parsed);
        Assert.IsTrue(parsed >= before);
    }
}
