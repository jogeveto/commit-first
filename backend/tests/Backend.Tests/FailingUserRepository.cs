using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Repositorio que simula un fallo de persistencia en el alta (HU-002 AC2).
/// FindByLinkedinSubAsync no encuentra cuenta (primer acceso) y CreateAsync falla
/// al escribir, como lo haría un error de base de datos durante el alta.
///
/// `Count` cuenta el almacén REAL de este doble (no un valor fijo), para que el
/// assert de "sin cuenta parcial creada" tenga contenido: si alguien cambiara el
/// orden y la fila se guardara antes de fallar, el contador lo delataría.
public class FailingUserRepository : IUserRepository
{
    private readonly Dictionary<string, Guid> _almacen = new();

    public int Count => _almacen.Count;

    public Task<Guid?> FindByLinkedinSubAsync(string linkedinSub)
        => Task.FromResult(_almacen.TryGetValue(linkedinSub, out var id) ? id : (Guid?)null);

    public Task<Guid> CreateAsync(string linkedinSub, string? name)
    {
        // La escritura falla ANTES de confirmarse: el almacén no se toca.
        throw new InvalidOperationException("fallo de persistencia simulado");
    }
}
