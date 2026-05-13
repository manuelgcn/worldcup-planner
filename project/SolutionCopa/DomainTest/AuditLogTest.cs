using ProjectCopa;
using ProjectCopa.Exceptions;

namespace DomainTest;

[TestClass]
public sealed class AuditLogTests
{
    private AuditLog _auditLog = null!;

    private const string ValidAction = "  TestAction  ";
    private const string ValidDetails = "This is a test audit log entry.";
    private const int ValidSettingsId = 99;
    private const string ValidEntityType = "Match";
    private const int ValidEntityId = 42;

    [TestInitialize]
    public void Setup()
    {
        _auditLog = AuditLog.Create(
            ValidAction,
            ValidDetails,
            ValidSettingsId,
            ValidEntityType,
            ValidEntityId);
    }

    [TestMethod]
    public void Create_ValidData_SetsActionTrimmedAndUpperCased()
    {
        Assert.AreEqual("TestAction", _auditLog.Action);
    }

    [TestMethod]
    public void Create_ValidData_SetsDetailsCorrectly()
    {
        Assert.AreEqual(ValidDetails, _auditLog.Details);
    }

    [TestMethod]
    public void Create_ValidData_SetsTournamentSettingsId()
    {
        Assert.AreEqual(ValidSettingsId, _auditLog.TournamentSettingsId);
    }

    [TestMethod]
    public void Create_ValidData_SetsAffectedEntityData()
    {
        Assert.AreEqual(ValidEntityType, _auditLog.AffectedEntityType);
        Assert.AreEqual(ValidEntityId, _auditLog.AffectedEntityId);
    }

    [TestMethod]
    public void Create_ValidData_SetsTimestampToCurrentUtc()
    {
        double secondsDifference = (DateTime.UtcNow - _auditLog.Timestamp).TotalSeconds;
        Assert.IsTrue(secondsDifference < 5, "Timestamp should be recent");
    }

    [TestMethod]
    public void Create_NullDetails_SetsDetailsToEmptyString()
    {
        AuditLog logWithNullDetails = AuditLog.Create("Action", null, 1);
        Assert.AreEqual(string.Empty, logWithNullDetails.Details);
    }

    [TestMethod]
    public void Create_NullAction_ThrowsAuditLogException()
    {
        Assert.ThrowsException<AuditLogException>(() => 
            AuditLog.Create(null, ValidDetails, ValidSettingsId));
    }

    [TestMethod]
    public void Create_EmptyOrWhitespaceAction_ThrowsAuditLogException()
    {
        Assert.ThrowsException<AuditLogException>(() => 
            AuditLog.Create("   ", ValidDetails, ValidSettingsId));
    }
    
    [TestMethod]
    public void SetId_ValidId_UpdatesIdProperty()
    {
        int expectedId = 15;
        _auditLog.SetId(expectedId); 
        Assert.AreEqual(expectedId, _auditLog.Id);
    }
}