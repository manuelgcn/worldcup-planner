using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.Exceptions;
using WorldCupPlanner.Services.Interfaces;

namespace WorldCupPlanner.UI;

// La sesión vive en el circuito de Blazor, que se pierde con cada F5 o link directo. Para que eso no
// cierre la sesión, el id del usuario se guarda en sessionStorage cifrado con Data Protection
// (el navegador no puede leerlo ni falsificarlo) y se descarta al cerrar la pestaña o a las 8 horas.
public class SessionState
{
    private const string SessionKey = "golazo.session";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

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
            ProtectedBrowserStorageResult<StoredSession> stored = await storage.GetAsync<StoredSession>(SessionKey);

            if (!stored.Success)
            {
                return;
            }

            // Una copia del valor cifrado sirve igual que el original: el vencimiento acota cuánto.
            if (stored.Value!.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await storage.DeleteAsync(SessionKey);
                return;
            }

            // Restaurar un backup reasigna ids: el email confirma que el id sigue siendo la misma persona.
            User user = userService.GetUserById(stored.Value.UserId);
            if (!string.Equals(user.email, stored.Value.Email, StringComparison.OrdinalIgnoreCase))
            {
                await storage.DeleteAsync(SessionKey);
                return;
            }

            CurrentUser = user;
        }
        catch (Exception ex) when (ex is CryptographicException or UserServicesException or JsonException)
        {
            await storage.DeleteAsync(SessionKey);
        }
    }

    // MainLayout lo llama en cada navegación y las acciones de administración antes de ejecutarse: si a
    // este usuario lo borraron o le cambiaron los roles en otra pestaña, la copia en memoria se actualiza.
    public async Task RevalidateAsync()
    {
        if (CurrentUser is null)
        {
            return;
        }

        try
        {
            CurrentUser = userService.GetUserById(CurrentUser.idUser);
        }
        catch (UserServicesException)
        {
            await LogoutAsync();
        }
    }

    public async Task LoginAsync(User user)
    {
        CurrentUser = user;
        await storage.SetAsync(SessionKey, new StoredSession(user.idUser, user.email, DateTime.UtcNow.Add(SessionLifetime)));
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
        await storage.DeleteAsync(SessionKey);
        NotifyStateChanged();
    }

    private sealed record StoredSession(int UserId, string Email, DateTime ExpiresAtUtc);

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
