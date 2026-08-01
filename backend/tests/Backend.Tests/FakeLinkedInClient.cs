using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Doble determinista de la frontera LinkedIn (D1). Dado cualquier `code`, retorna
/// el perfil configurado. No es un mock de verificación: modela el comportamiento
/// del proveedor para ejercer los AC sin red ni credenciales reales.
///
/// `ExchangeCount` permite comprobar el ORDEN de las operaciones: un callback con
/// `state` inválido no debe llegar nunca a canjear el `code` contra el proveedor.
public class FakeLinkedInClient : ILinkedInClient
{
    private readonly LinkedInProfile _profile;
    public FakeLinkedInClient(LinkedInProfile profile) => _profile = profile;

    public int ExchangeCount { get; private set; }

    public Task<LinkedInProfile> ExchangeCodeAsync(string code)
    {
        ExchangeCount++;
        return Task.FromResult(_profile);
    }
}
