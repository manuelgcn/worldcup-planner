using ProjectCopa;

namespace DomainTest;

[TestClass]
public class AuditLogTest
{
    private AuditLog _auditLog = null!;
    
    [TestMethod]
    public void CreateAuditLogEntry()
    {
        InitiateAuditLog();

        // Act & Assert
        Assert.AreEqual("TestAction", _auditLog.Action);
        Assert.AreEqual("This is a test audit log entry.", _auditLog.Details);
        Assert.AreEqual(1, _auditLog.TournamentSettingsId);
        Assert.AreEqual(1, _auditLog.AffectedEntityId);
        Assert.IsTrue((DateTime.UtcNow - _auditLog.Timestamp).TotalSeconds < 5); // Timestamp should be recent
    }

    private void InitiateAuditLog()
    {
        _auditLog = AuditLog.Create(
            action: "TestAction",
            details: "This is a test audit log entry.",
            tournamentSettingsId: 1,
            affectedEntityType: "Match",
            affectedEntityId: 1);
    }
}