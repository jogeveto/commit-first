using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Empleabilidad.Api.Auth;

/// Almacén de `state` OAuth2 en memoria (dev / instancia única). Genera un `state`
/// aleatorio criptográfico al iniciar el flujo y lo consume una sola vez en el
/// callback (single-use → mitiga replay).
///
/// Con TTL: un flujo que se inicia y nunca se completa —el usuario cierra la
/// pestaña, o alguien golpea el endpoint de inicio— dejaba antes su `state`
/// almacenado para siempre, de modo que el diccionario crecía sin límite. Ahora
/// los `state` caducan y se purgan, y uno caducado se rechaza como si no existiera.
///
/// Para producción multi-instancia se sustituye por cache distribuida o cookie
/// firmada; el contrato IAuthStateStore no cambia.
public class InMemoryAuthStateStore : IAuthStateStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _emitidos = new();
    private readonly TimeSpan _vigencia;
    private readonly Func<DateTimeOffset> _ahora;

    /// El reloj es inyectable para poder probar la caducidad sin esperas reales.
    public InMemoryAuthStateStore(TimeSpan? vigencia = null, Func<DateTimeOffset>? ahora = null)
    {
        _vigencia = vigencia ?? TimeSpan.FromMinutes(10);
        _ahora = ahora ?? (() => DateTimeOffset.UtcNow);
    }

    public int Pendientes => _emitidos.Count;

    public string Issue()
    {
        PurgarCaducados();
        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _emitidos[state] = _ahora().Add(_vigencia);
        return state;
    }

    public bool Validate(string? state)
    {
        if (state is null) return false;
        if (!_emitidos.TryRemove(state, out var caducaEn)) return false;
        return caducaEn > _ahora(); // un `state` caducado no vale, y ya quedó consumido
    }

    private void PurgarCaducados()
    {
        var ahora = _ahora();
        foreach (var (state, caducaEn) in _emitidos)
            if (caducaEn <= ahora)
                _emitidos.TryRemove(state, out _);
    }
}
