using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Empleabilidad.Api.Auth;

/// Almacén de `state` OAuth2 en memoria (dev / single-instance). Genera un `state`
/// aleatorio criptográfico al iniciar el flujo y lo consume una sola vez en el
/// callback (single-use → mitiga replay). Para producción multi-instancia se
/// sustituye por cache distribuida o cookie firmada; el contrato IAuthStateStore
/// no cambia. Registrado como mejora de robustez (no bloquea el comportamiento).
public class InMemoryAuthStateStore : IAuthStateStore
{
    private readonly ConcurrentDictionary<string, byte> _issued = new();

    public string Issue()
    {
        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _issued[state] = 0;
        return state;
    }

    public bool Validate(string? state)
        => state is not null && _issued.TryRemove(state, out _);
}
