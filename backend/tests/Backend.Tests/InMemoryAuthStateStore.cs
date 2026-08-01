using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Almacén real (en memoria) de `state` para tests. Issue memoriza; Validate
/// consume el `state` una sola vez. Modela la semántica CSRF (single-use) sin
/// infraestructura. La idempotencia de un `state` no se reutiliza: tras validar,
/// desaparece.
public class InMemoryAuthStateStore : IAuthStateStore
{
    private readonly HashSet<string> _issued = new();
    private int _counter;

    public string Issue()
    {
        var state = $"state-{++_counter}";
        _issued.Add(state);
        return state;
    }

    public bool Validate(string? state)
        => state is not null && _issued.Remove(state);
}
