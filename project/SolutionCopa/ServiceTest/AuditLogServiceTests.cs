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
    public void CreateInstance_ReturnsServiceThatCanLog()
    {
        // Act
        _service.Log("CREATE", "Match Created", 1, "Match", 10);

        // Assert
        Assert.AreEqual(1, _repository.GetAll().Count);
        Assert.AreEqual("CREATE", _repository.GetAll()[0].Action);
    }

    [TestMethod]
    public void Log_WithExplicitTournament_KeepsOptionalContext()
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
    public void Log_WithoutTournament_CreatesGlobalLog()
    {
        // Act
        _service.Log("EXPORT", "Data exported", "Export", 7);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.IsNull(logs[0].TournamentSettingsId);
    }

    [TestMethod]
    public void Log_WithoutTournament_StoresEntityData()
    {
        // Act
        _service.Log("EXPORT", "Data exported", "Export", 7);

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual("Export", logs[0].AffectedEntityType);
        Assert.AreEqual(7, logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void Log_WithoutEntityData_StoresNullEntityData()
    {
        // Act
        _service.Log("EXPORT", "Data exported");

        // Assert
        List<AuditLog> logs = _repository.GetAll();
        Assert.IsNull(logs[0].AffectedEntityType);
        Assert.IsNull(logs[0].AffectedEntityId);
    }

    [TestMethod]
    public void LogPerformedBy_StoresActorInPerformedByEmail()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: "admin@copa.com",
            affectedEntityType: "Fixture");

        List<AuditLog> logs = _repository.GetAll();
        Assert.AreEqual(1, logs.Count);
        Assert.AreEqual("admin@copa.com", logs[0].PerformedByEmail);
    }

    [TestMethod]
    public void LogPerformedBy_DoesNotConcatenateActorIntoDetails()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: "admin@copa.com");

        Assert.AreEqual("Fixture generated", _repository.GetAll()[0].Details);
    }

    [TestMethod]
    public void LogPerformedBy_CreatesGlobalLog()
    {
        _service.LogPerformedBy(
            AuditActions.SimulateRound,
            "Round simulated",
            performedByEmail: "admin@copa.com");

        Assert.IsNull(_repository.GetAll()[0].TournamentSettingsId);
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
    public void LogPerformedBy_WithNullEmail_LeavesActorNull()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: null);

        Assert.IsNull(_repository.GetAll()[0].PerformedByEmail);
    }

    [TestMethod]
    public void LogPerformedBy_WithWhitespaceEmail_LeavesActorNull()
    {
        _service.LogPerformedBy(
            AuditActions.GenerateFixture,
            "Fixture generated",
            performedByEmail: "   ");

        Assert.IsNull(_repository.GetAll()[0].PerformedByEmail);
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtosWithCorrectCount()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION1", "Det 1", 1));
        _repository.Seed(AuditLog.Create("ACTION2", "Det 2", 2));

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAuditLogDtosWithCorrectActions()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION1", "Det 1", 1));
        _repository.Seed(AuditLog.Create("ACTION2", "Det 2", 2));

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        CollectionAssert.AreEqual(new List<string> { "ACTION1", "ACTION2" }, result.Select(dto => dto.Action).ToList());
    }

    [TestMethod]
    public void GetAll_MapsNullableTournamentSettingsId()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("WITH_TOURNAMENT", "Det 1", 1));
        _repository.Seed(AuditLog.Create("GLOBAL", "Det 2"));

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        Assert.AreEqual(1, result[0].TournamentSettingsId);
        Assert.IsNull(result[1].TournamentSettingsId);
    }

    [TestMethod]
    public void GetAll_MapsPerformedByEmail()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION", "Det", performedByEmail: "admin@copa.com"));

        // Act
        List<AuditLogDto> result = _service.GetAll();

        // Assert
        Assert.AreEqual("admin@copa.com", result[0].PerformedByEmail);
    }

    [TestMethod]
    public void GetFiltered_WithEmptyFilter_ReturnsAllLogs()
    {
        _repository.Seed(AuditLog.Create("ACTION1", "Det 1"));
        _repository.Seed(AuditLog.Create("ACTION2", "Det 2"));

        List<AuditLogDto> result = _service.GetFiltered(new AuditLogFilterDto());

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetFiltered_ByAction_ReturnsPartialCaseInsensitiveMatches()
    {
        _repository.Seed(AuditLog.Create("CREATE_USER", "Det 1"));
        _repository.Seed(AuditLog.Create("DELETE_TEAM", "Det 2"));

        List<AuditLogDto> result = _service.GetFiltered(new AuditLogFilterDto { Action = "create" });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CREATE_USER", result[0].Action);
    }

    [TestMethod]
    public void GetFiltered_ByActor_ReturnsPartialCaseInsensitiveMatches()
    {
        _repository.Seed(AuditLog.Create("ACTION", "Det 1", performedByEmail: "Admin@copa.com"));
        _repository.Seed(AuditLog.Create("ACTION", "Det 2", performedByEmail: "editor@copa.com"));

        List<AuditLogDto> result = _service.GetFiltered(new AuditLogFilterDto { PerformedByEmail = "ADMIN" });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Admin@copa.com", result[0].PerformedByEmail);
    }

    [TestMethod]
    public void GetFiltered_ByInclusiveDateRange_ExcludesLogsOutsideRange()
    {
        AuditLog log = AuditLog.Create("ACTION", "Det 1");
        _repository.Seed(log);

        List<AuditLogDto> inside = _service.GetFiltered(new AuditLogFilterDto
        {
            From = log.Timestamp,
            To = log.Timestamp
        });
        List<AuditLogDto> outside = _service.GetFiltered(new AuditLogFilterDto
        {
            From = log.Timestamp.AddSeconds(1)
        });

        Assert.AreEqual(1, inside.Count);
        Assert.AreEqual(0, outside.Count);
    }

    [TestMethod]
    public void GetFiltered_CombinesDateActionAndActor()
    {
        AuditLog match = AuditLog.Create("CREATE_USER", "match", performedByEmail: "admin@copa.com");
        _repository.Seed(match);
        _repository.Seed(AuditLog.Create("CREATE_USER", "wrong actor", performedByEmail: "editor@copa.com"));
        _repository.Seed(AuditLog.Create("DELETE_TEAM", "wrong action", performedByEmail: "admin@copa.com"));

        List<AuditLogDto> result = _service.GetFiltered(new AuditLogFilterDto
        {
            From = match.Timestamp.AddSeconds(-1),
            To = match.Timestamp.AddSeconds(1),
            Action = "CREATE",
            PerformedByEmail = "admin"
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("match", result[0].Details);
    }

    [TestMethod]
    public void ExportAsText_ReturnsAllLogsActions()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION1", "Det 1", 1));
        _repository.Seed(AuditLog.Create("ACTION2", "Det 2"));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("ACTION1"));
        Assert.IsTrue(result.Contains("ACTION2"));
    }

    [TestMethod]
    public void ExportAsText_WithTournamentContext_IncludesTournamentId()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION1", "Det 1", 1));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("TournamentID: 1"));
    }

    [TestMethod]
    public void ExportAsText_GlobalLog_OmitsTournamentId()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("GLOBAL", "Det 1"));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsFalse(result.Contains("TournamentID:"));
    }

    [TestMethod]
    public void ExportAsText_WithActor_IncludesPerformedBy()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION", "Det 1", performedByEmail: "admin@copa.com"));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("PerformedBy: admin@copa.com"));
    }

    [TestMethod]
    public void ExportAsText_WithoutActor_OmitsPerformedBy()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("ACTION", "Det 1"));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsFalse(result.Contains("PerformedBy:"));
    }

    [TestMethod]
    public void ExportAsText_WithEntities_FormatsTextCorrectly()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("UPDATE", "Settings updated", 1, "TournamentSettings", 1));

        // Act
        string result = _service.ExportAsText();

        // Assert
        Assert.IsTrue(result.Contains("Entity: TournamentSettings - ID: 1"));
        Assert.IsTrue(result.Contains("Action: UPDATE"));
    }

    [TestMethod]
    public void ExportAsText_WithoutEntities_FormatsTextCorrectly()
    {
        // Arrange
        _repository.Seed(AuditLog.Create("EXPORT", "Full Export"));

        // Act
        string result = _service.ExportAsText();

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
    public void GetAll_PreservesInsertionOrder()
    {
        _service.Log("FIRST", "first action");
        _service.Log("SECOND", "second action");
        _service.Log("THIRD", "third action");

        List<AuditLogDto> result = _service.GetAll();

        CollectionAssert.AreEqual(
            new List<string> { "FIRST", "SECOND", "THIRD" },
            result.Select(dto => dto.Action).ToList());
    }

    [TestMethod]
    public void Log_StoresIso8601RoundTrippableTimestamp()
    {
        DateTime before = DateTime.UtcNow.AddSeconds(-1);

        _service.Log("ACTION", "Details");

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
