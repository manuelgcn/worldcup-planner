using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Domain.Tests;

[TestClass]
public sealed class UserRolesTests
{
    [TestMethod]
    public void UserRolesContainsPeriodista()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(UserRoles), UserRoles.Periodista));
    }

    [TestMethod]
    public void UserRolesPreservesExistingOrdinals()
    {
        Assert.AreEqual(0, (int)UserRoles.Admin);
        Assert.AreEqual(1, (int)UserRoles.Editor);
        Assert.AreEqual(2, (int)UserRoles.Periodista);
    }
}
