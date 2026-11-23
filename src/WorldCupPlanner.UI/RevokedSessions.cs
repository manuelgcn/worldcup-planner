using System.Collections.Concurrent;

namespace WorldCupPlanner.UI;

// Sesiones cerradas con "Cerrar sesión" que todavía no vencieron. Una pestaña duplicada guarda una copia
// válida del valor cifrado, así que solo el servidor puede saber que esa sesión ya se cerró.
// shortcut: vive en memoria y por servidor, como el bloqueo de login: reiniciar la app o atender con otra
// instancia olvida los cierres; guardarlas en la base cuando haya más de una instancia.
public sealed class RevokedSessions
{
    private readonly ConcurrentDictionary<Guid, DateTime> expiryBySessionId = new();

    // Cuántas sesiones cerradas se recuerdan: deja comprobar que esa memoria está acotada.
    public int Count => expiryBySessionId.Count;

    public bool Contains(Guid sessionId) => expiryBySessionId.ContainsKey(sessionId);

    // Una sesión vencida ya no entra aunque la copien: se olvida, y así solo quedan los cierres de las últimas horas.
    public void Add(Guid sessionId, DateTime expiresAtUtc, DateTime nowUtc)
    {
        foreach (KeyValuePair<Guid, DateTime> entry in expiryBySessionId)
        {
            if (entry.Value <= nowUtc)
                expiryBySessionId.TryRemove(entry);
        }

        if (expiresAtUtc > nowUtc)
            expiryBySessionId[sessionId] = expiresAtUtc;
    }
}
