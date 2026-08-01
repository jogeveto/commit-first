using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Implementación real (en memoria) de IUserRepository para tests unitarios de la
/// lógica de orquestación. NO es un mock: almacena de verdad. La idempotencia se
/// prueba sobre AccountProvisioningService, no sobre este almacén.
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, Guid> _byLinkedinSub = new();

    public int Count => _byLinkedinSub.Count;

    public Task<Guid?> FindByLinkedinSubAsync(string linkedinSub)
        => Task.FromResult(_byLinkedinSub.TryGetValue(linkedinSub, out var id) ? id : (Guid?)null);

    public Task<Guid> CreateAsync(string linkedinSub, string? name)
    {
        var id = Guid.NewGuid();
        _byLinkedinSub[linkedinSub] = id;
        return Task.FromResult(id);
    }
}
