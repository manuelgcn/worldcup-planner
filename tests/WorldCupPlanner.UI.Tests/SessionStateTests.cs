using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
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

    // Un servidor: todas las pestañas comparten sus claves de cifrado, su reloj y su servicio de usuarios.
    private readonly EphemeralDataProtectionProvider dataProtection = new();
    private readonly ManualTimeProvider clock = new();
    private readonly Mock<IUserService> userService = new();
    private readonly InMemorySessionStorageJsRuntime tab = new();

    private static User BuildUser(string email = Email, UserRoles role = UserRoles.Editor)
    {
        User user = new User { idUser = UserId, name = "Ana", lastname = "Pérez", email = email };
        user.AddRole(role);
        return user;
    }

    // Cada circuito de Blazor (una carga de página) tiene su propio SessionState sobre el sessionStorage de la pestaña.
    private SessionState OpenCircuit(InMemorySessionStorageJsRuntime browserTab) =>
        new SessionState(new ProtectedSessionStorage(browserTab, dataProtection), userService.Object, clock);

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
}
