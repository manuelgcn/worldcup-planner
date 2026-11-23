using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;
using WorldCupPlanner.UI.Tests.TestDoubles;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class SessionStateTests
{
    private const int UserId = 7;
    private const string Email = "ana@copa.com";

    // Un servidor: todas las pestañas comparten sus claves de cifrado y su servicio de usuarios.
    private readonly EphemeralDataProtectionProvider dataProtection = new();
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
        new SessionState(new ProtectedSessionStorage(browserTab, dataProtection), userService.Object);

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
}
