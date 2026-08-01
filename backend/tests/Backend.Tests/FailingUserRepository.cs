using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Repositorio cuya escritura falla, para probar cómo REACCIONA la orquestación
/// (HU-002 AC2): no emite sesión y devuelve ProvisioningFailed.
///
/// Deliberadamente NO expone un contador de cuentas. Un `Count` sobre este doble
/// sería un assert vacuo: quien decide si la fila se escribe es el propio doble,
/// no el código bajo prueba, así que daría 0 con cualquier comportamiento. La parte
/// del AC que dice "sin cuenta parcial creada" se verifica contra PostgreSQL real
/// en PostgresUserRepositoryTests.Fallo_durante_el_alta_no_emite_sesion_ni_deja_cuenta.
public class FailingUserRepository : IUserRepository
{
    public Task<Guid?> FindByLinkedinSubAsync(string linkedinSub)
        => Task.FromResult<Guid?>(null);

    public Task<Guid> CreateAsync(string linkedinSub, string? name)
        => throw new InvalidOperationException("fallo de persistencia simulado");
}
