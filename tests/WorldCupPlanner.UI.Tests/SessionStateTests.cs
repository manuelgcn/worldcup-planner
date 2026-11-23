using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.Services.Tests.TestDoubles.Mocks;
using WorldCupPlanner.UI.Tests.TestDoubles;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class SessionStateTests
{
    private const int UserId = 7;
    private const string Email = "ana@copa.com";
    private const string NewEmail = "ana.perez@copa.com";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    // Un servidor: todas las pestañas comparten sus claves de cifrado, su reloj, sus sesiones cerradas y su servicio de usuarios.
    private readonly EphemeralDataProtectionProvider dataProtection = new();
    private readonly ManualTimeProvider clock = new();
    private readonly RevokedSessions revokedSessions = new();
    private readonly Mock<IUserService> userService = new();
    private readonly Mock<ILogger<SessionState>> logger = new();
    private readonly InMemorySessionStorageJsRuntime tab = new();

    private static User BuildUser(string email = Email, UserRoles role = UserRoles.Editor)
    {
        User user = new User { idUser = UserId, name = "Ana", lastname = "Pérez", email = email };
        user.AddRole(role);
        return user;
    }

    // Cada circuito de Blazor (una carga de página) tiene su propio SessionState sobre el sessionStorage de la pestaña.
    private SessionState OpenCircuit(InMemorySessionStorageJsRuntime browserTab) =>
        new SessionState(new ProtectedSessionStorage(browserTab, dataProtection), userService.Object, clock, logger.Object, revokedSessions);

    private async Task<SessionState> LoginAsync()
    {
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser());
        SessionState session = OpenCircuit(tab);
        await session.LoginAsync(BuildUser());
        return session;
    }

    [TestMethod]
    public async Task RestoreAsync_AfterReloadingALoggedInTab_RestoresTheUser()
    {
        await LoginAsync();

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsTrue(reloaded.IsLoggedIn);
        Assert.AreEqual(Email, reloaded.CurrentUser!.email);
    }

    [TestMethod]
    public async Task RestoreAsync_InATabThatNeverLoggedIn_StaysLoggedOut()
    {
        SessionState session = OpenCircuit(tab);

        await session.RestoreAsync();

        Assert.IsFalse(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task RestoreAsync_AfterLogout_StaysLoggedOut()
    {
        SessionState session = await LoginAsync();
        await session.LogoutAsync();

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsFalse(reloaded.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_WhenTheIdNowBelongsToAnotherEmail_DiscardsTheSession()
    {
        await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser("otra@copa.com"));

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsFalse(reloaded.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_WhenTheUserWasRemoved_DiscardsTheSession()
    {
        await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Throws(new UserServicesException("Usuario no encontrado"));

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsFalse(reloaded.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_WhenTheServerHasOtherEncryptionKeys_DiscardsTheSession()
    {
        await LoginAsync();
        SessionState restarted = new SessionState(
            new ProtectedSessionStorage(tab, new EphemeralDataProtectionProvider()),
            userService.Object,
            clock,
            logger.Object,
            revokedSessions);

        await restarted.RestoreAsync();

        Assert.IsFalse(restarted.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RevalidateAsync_WhenTheIdNowBelongsToAnotherEmail_LogsOut()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser("otra@copa.com"));

        await session.RevalidateAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RevalidateAsync_WhenARoleChangedInAnotherTab_ReadsTheUserAgain()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser(role: UserRoles.Periodista));

        await session.RevalidateAsync();

        Assert.IsTrue(session.IsJournalist);
    }

    [TestMethod]
    public async Task RevalidateAsync_WhenTheUserWasRemoved_LogsOut()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Throws(new UserServicesException("Usuario no encontrado"));

        await session.RevalidateAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_EightHoursAfterLogin_DiscardsTheSession()
    {
        await LoginAsync();
        clock.Advance(SessionLifetime);

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsFalse(reloaded.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RevalidateAsync_EightHoursAfterLoginInATabLeftOpen_LogsOut()
    {
        SessionState session = await LoginAsync();
        clock.Advance(SessionLifetime);

        await session.RevalidateAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task RevalidateAsync_JustBeforeEightHours_KeepsTheUser()
    {
        SessionState session = await LoginAsync();
        clock.Advance(SessionLifetime - TimeSpan.FromSeconds(1));

        await session.RevalidateAsync();

        Assert.IsTrue(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task RevalidateAsync_EightHoursAfterLoginInAReloadedTab_LogsOut()
    {
        await LoginAsync();
        clock.Advance(TimeSpan.FromHours(7));
        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();
        clock.Advance(TimeSpan.FromHours(1));

        await reloaded.RevalidateAsync();

        Assert.IsFalse(reloaded.IsLoggedIn);
    }

    [TestMethod]
    public async Task LoginAsync_Again_StartsANewEightHourWindow()
    {
        SessionState session = await LoginAsync();
        clock.Advance(TimeSpan.FromHours(7));
        await session.LoginAsync(BuildUser());
        clock.Advance(TimeSpan.FromHours(7));

        await session.RevalidateAsync();

        Assert.IsTrue(session.IsLoggedIn);
    }

    // Sin esto, "Cerrar sesión" después de volver a ingresar solo cerraría la sesión nueva y la
    // pestaña duplicada seguiría con la anterior.
    [TestMethod]
    public async Task LoginAsync_Again_EndsThePreviousSessionInADuplicatedTab()
    {
        SessionState session = await LoginAsync();
        SessionState duplicate = OpenCircuit(tab.CopyTab());
        await duplicate.RestoreAsync();

        await session.LoginAsync(BuildUser());
        await duplicate.RevalidateAsync();
        await session.RevalidateAsync();

        Assert.IsFalse(duplicate.IsLoggedIn);
        Assert.IsTrue(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task RefreshUserAsync_AfterAnEmailChange_KeepsTheSessionInThisCircuitAndAfterAReload()
    {
        SessionState session = await LoginAsync();
        int changes = 0;
        session.OnChange += () => changes++;
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser(NewEmail));

        await session.RefreshUserAsync();

        Assert.AreEqual(NewEmail, session.CurrentUser!.email);
        Assert.AreEqual(1, changes);

        await session.RevalidateAsync();
        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsTrue(session.IsLoggedIn);
        Assert.IsTrue(reloaded.IsLoggedIn);
    }

    // La pestaña duplicada comparte el id de sesión y todavía tiene el email viejo: al descartar su copia
    // no puede cerrar la sesión de la pestaña donde el usuario se cambió el email.
    [TestMethod]
    public async Task RevalidateAsync_InADuplicatedTabAfterAnEmailChange_DoesNotEndTheTabThatChangedIt()
    {
        SessionState session = await LoginAsync();
        SessionState duplicate = OpenCircuit(tab.CopyTab());
        await duplicate.RestoreAsync();
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser(NewEmail));
        await session.RefreshUserAsync();

        await duplicate.RevalidateAsync();
        await session.RevalidateAsync();

        Assert.IsFalse(duplicate.IsLoggedIn);
        Assert.IsTrue(session.IsLoggedIn);
        Assert.AreEqual(0, revokedSessions.Count);
    }

    [TestMethod]
    public async Task RefreshUserAsync_DoesNotRestartTheEightHours()
    {
        SessionState session = await LoginAsync();
        clock.Advance(TimeSpan.FromHours(7));
        await session.RefreshUserAsync();
        clock.Advance(TimeSpan.FromHours(1));

        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();
        await session.RevalidateAsync();

        Assert.IsFalse(reloaded.IsLoggedIn);
        Assert.IsFalse(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task RefreshUserAsync_WhenLoggedOut_DoesNothing()
    {
        SessionState session = OpenCircuit(tab);

        await session.RefreshUserAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
        userService.Verify(service => service.GetUserById(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task RevalidateAsync_WhenReadingTheUserFails_KeepsTheUserAndLogsTheError()
    {
        SessionState session = await LoginAsync();
        InvalidOperationException databaseError = new InvalidOperationException("database unavailable");
        userService.Setup(service => service.GetUserById(UserId)).Throws(databaseError);

        await session.RevalidateAsync();

        Assert.IsTrue(session.IsLoggedIn);
        Assert.IsFalse(tab.IsEmpty);
        logger.Verify(
            log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                databaseError,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task LogoutAsync_EndsTheSessionInADuplicatedTab()
    {
        SessionState session = await LoginAsync();
        InMemorySessionStorageJsRuntime duplicatedTab = tab.CopyTab();
        SessionState duplicate = OpenCircuit(duplicatedTab);
        await duplicate.RestoreAsync();

        await session.LogoutAsync();
        await duplicate.RevalidateAsync();

        Assert.IsFalse(duplicate.IsLoggedIn);
        Assert.IsTrue(duplicatedTab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_FromACopyOfALoggedOutSession_StaysLoggedOut()
    {
        SessionState session = await LoginAsync();
        InMemorySessionStorageJsRuntime copiedTab = tab.CopyTab();
        await session.LogoutAsync();

        SessionState copy = OpenCircuit(copiedTab);
        await copy.RestoreAsync();

        Assert.IsFalse(copy.IsLoggedIn);
        Assert.IsTrue(copiedTab.IsEmpty);
    }

    [TestMethod]
    public async Task RestoreAsync_OfASessionStoredWithoutAnId_DiscardsIt()
    {
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser());
        await new ProtectedSessionStorage(tab, dataProtection).SetAsync(
            "golazo.session",
            new { UserId, Email, ExpiresAtUtc = clock.GetUtcNow().UtcDateTime.AddHours(1) });

        SessionState session = OpenCircuit(tab);
        await session.RestoreAsync();

        Assert.IsFalse(session.IsLoggedIn);
        Assert.IsTrue(tab.IsEmpty);
    }

    [TestMethod]
    public async Task LoginAsync_AfterLogout_StartsASessionThatIsNotRevoked()
    {
        SessionState session = await LoginAsync();
        await session.LogoutAsync();
        await session.LoginAsync(BuildUser());

        await session.RevalidateAsync();
        SessionState reloaded = OpenCircuit(tab);
        await reloaded.RestoreAsync();

        Assert.IsTrue(session.IsLoggedIn);
        Assert.IsTrue(reloaded.IsLoggedIn);
    }

    [TestMethod]
    public async Task LogoutAsync_DoesNotEndAnotherLoginOfTheSameUser()
    {
        SessionState session = await LoginAsync();
        SessionState otherLogin = OpenCircuit(new InMemorySessionStorageJsRuntime());
        await otherLogin.LoginAsync(BuildUser());

        await session.LogoutAsync();
        await otherLogin.RevalidateAsync();

        Assert.IsTrue(otherLogin.IsLoggedIn);
    }

    [TestMethod]
    public async Task LogoutAsync_ForgetsRevokedSessionsThatAlreadyExpired()
    {
        SessionState first = await LoginAsync();
        await first.LogoutAsync();
        int revokedAfterFirstLogout = revokedSessions.Count;
        clock.Advance(SessionLifetime);

        SessionState second = OpenCircuit(new InMemorySessionStorageJsRuntime());
        await second.LoginAsync(BuildUser());
        await second.LogoutAsync();

        Assert.AreEqual(1, revokedAfterFirstLogout);
        Assert.AreEqual(1, revokedSessions.Count);
    }

    [TestMethod]
    public async Task RevalidateAsync_OfAnExpiredSession_DoesNotRememberItAsRevoked()
    {
        SessionState session = await LoginAsync();
        clock.Advance(SessionLifetime);

        await session.RevalidateAsync();

        Assert.AreEqual(0, revokedSessions.Count);
    }

    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenTheUserHasOneOfTheRoles_IsTrue()
    {
        SessionState session = await LoginAsync();

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Admin, UserRoles.Editor);

        Assert.IsTrue(allowed);
    }

    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenTheRoleWasRemovedSinceThePageLoaded_IsFalse()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Returns(() => BuildUser(role: UserRoles.Periodista));

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Admin, UserRoles.Editor);

        Assert.IsFalse(allowed);
        Assert.IsTrue(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenTheUserWasRemoved_IsFalseAndLogsOut()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Throws(new UserServicesException("Usuario no encontrado"));

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Editor);

        Assert.IsFalse(allowed);
        Assert.IsFalse(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenTheSessionExpired_IsFalseAndLogsOut()
    {
        SessionState session = await LoginAsync();
        clock.Advance(SessionLifetime);

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Editor);

        Assert.IsFalse(allowed);
        Assert.IsFalse(session.IsLoggedIn);
    }

    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenNobodyIsLoggedIn_IsFalse()
    {
        SessionState session = OpenCircuit(tab);

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Admin, UserRoles.Editor, UserRoles.Periodista);

        Assert.IsFalse(allowed);
    }

    // El rol no se pudo confirmar: la sesión sigue, pero una acción protegida no se da por permitida.
    [TestMethod]
    public async Task StillHasAnyRoleAsync_WhenReadingTheUserFails_IsFalseAndKeepsTheSession()
    {
        SessionState session = await LoginAsync();
        userService.Setup(service => service.GetUserById(UserId)).Throws(new InvalidOperationException("database unavailable"));

        bool allowed = await session.StillHasAnyRoleAsync(UserRoles.Editor);

        Assert.IsFalse(allowed);
        Assert.IsTrue(session.IsLoggedIn);
    }
}
