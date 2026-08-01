namespace Empleabilidad.Api.Auth;

/// Implementación dev/test de la frontera LinkedIn (D1). Deriva un perfil
/// determinista del `code` recibido, permitiendo ejercer el flujo OAuth2 completo
/// sin credenciales ni red. Se inyecta cuando faltan LINKEDIN_CLIENT_ID/SECRET.
/// El `sub` estable por `code` permite verificar idempotencia (mismo code → misma cuenta).
public class FakeLinkedInClient : ILinkedInClient
{
    public Task<LinkedInProfile> ExchangeCodeAsync(string code)
        => Task.FromResult(new LinkedInProfile($"linkedin-sub-{code}", "Usuario Demo"));
}
