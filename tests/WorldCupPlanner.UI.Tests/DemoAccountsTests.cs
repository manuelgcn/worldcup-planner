using Microsoft.Extensions.Logging;
using Moq;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class DemoAccountsTests
{
    private readonly Mock<ILogger> logger = new();

    private static User BuildUser(string email, UserRoles role)
    {
        User user = new User { name = "Ana", lastname = "Pérez", email = email };
        user.AddRole(role);
        return user;
    }

    private void VerifyWarnings(Times times, string? about = null)
    {
        logger.Verify(
            log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => about == null || state.ToString()!.Contains(about)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    [TestMethod]
    public void VerifyDisabled_OnAnEmptyDatabase_RefusesToStart()
    {
        Assert.ThrowsExactly<InvalidOperationException>(
            () => DemoAccounts.VerifyDisabled(Array.Empty<User>(), logger.Object));
    }

    [TestMethod]
    public void VerifyDisabled_WhenNoUserIsAdmin_RefusesToStart()
    {
        User[] users = { BuildUser("ana@copa.com", UserRoles.Editor), BuildUser("luis@copa.com", UserRoles.Periodista) };

        Assert.ThrowsExactly<InvalidOperationException>(() => DemoAccounts.VerifyDisabled(users, logger.Object));
    }

    [TestMethod]
    public void VerifyDisabled_WithAnAdminAndNoDemoAccounts_StartsWithoutWarnings()
    {
        User[] users = { BuildUser("ana@copa.com", UserRoles.Admin), BuildUser("luis@copa.com", UserRoles.Periodista) };

        DemoAccounts.VerifyDisabled(users, logger.Object);

        VerifyWarnings(Times.Never());
    }

    // Apagar la opción no borra las cuentas que un arranque anterior creó, y su contraseña está en el README.
    [TestMethod]
    public void VerifyDisabled_WhenDemoAccountsStillExist_WarnsAboutEachOne()
    {
        User[] users =
        {
            BuildUser("admin@copa.com", UserRoles.Admin),
            BuildUser("Editor@Copa.com", UserRoles.Editor),
            BuildUser("ana@copa.com", UserRoles.Periodista)
        };

        DemoAccounts.VerifyDisabled(users, logger.Object);

        VerifyWarnings(Times.Exactly(2));
        VerifyWarnings(Times.Once(), about: "admin@copa.com");
        // El aviso nombra la cuenta de demo como está publicada, no lo que guarda la fila del usuario.
        VerifyWarnings(Times.Once(), about: "editor@copa.com");
    }
}
