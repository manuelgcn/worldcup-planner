using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.UI;

// La sesión vive en el circuito de Blazor, que se pierde con cada F5 o link directo. Para que eso no
// cierre la sesión, el id del usuario se guarda en sessionStorage cifrado con Data Protection
// (el navegador no puede leerlo ni falsificarlo) y se descarta al cerrar la pestaña.
public class SessionState
{
    private const string UserIdKey = "golazo.userId";

    private readonly ProtectedSessionStorage storage;
    private readonly IUserService userService;

    public SessionState(ProtectedSessionStorage storage, IUserService userService)
    {
        this.storage = storage;
        this.userService = userService;
    }

    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public bool IsJournalist => CurrentUser?.HasRole(UserRoles.Periodista) == true;

    public event Action? OnChange;

    // Routes.razor lo llama una vez por circuito, antes de mostrar cualquier página. El usuario se relee
    // de la base para tomar sus roles actuales; si ya no existe o cambiaron las claves de cifrado, se descarta.
    public async Task RestoreAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<int> stored = await storage.GetAsync<int>(UserIdKey);

            if (stored.Success)
            {
                CurrentUser = userService.GetUserById(stored.Value);
            }
        }
        catch (Exception ex) when (ex is CryptographicException or UserServicesException)
        {
            await storage.DeleteAsync(UserIdKey);
        }
    }

    public async Task LoginAsync(User user)
    {
        CurrentUser = user;
        await storage.SetAsync(UserIdKey, user.idUser);
        NotifyStateChanged();
    }

    // Avisa al menú y al inicio que cambió algo que muestran de la sesión (p. ej. las notificaciones sin leer).
    public void Refresh()
    {
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await storage.DeleteAsync(UserIdKey);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
