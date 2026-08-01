namespace Empleabilidad.Api.Auth;

/// Frontera hacia LinkedIn (D1, mock-first). Intercambia el `code` del callback
/// OAuth2 por el perfil del usuario. FakeLinkedInClient (dev/test) es determinista;
/// RealLinkedInClient (prod) hace el intercambio real cuando hay credenciales.
/// Aísla el dominio del servicio no determinista de terceros.
public interface ILinkedInClient
{
    Task<LinkedInProfile> ExchangeCodeAsync(string code);
}
