namespace Empleabilidad.Api.Auth;

/// Configuración del flujo OAuth2 de LinkedIn y las decisiones que dependen de ella.
///
/// Esta lógica vivía dentro de `Program.cs`, donde ningún test la alcanzaba: se
/// podía inyectar el cliente falso aun teniendo credenciales reales (suplantación
/// trivial: cualquier `code` produciría un perfil válido) o construir la URL de
/// autorización sin `state` (CSRF desactivado en producción), y toda la suite
/// seguía en verde. Al extraerla a funciones puras, cada decisión es verificable.
public record LinkedInOAuthOptions(string? ClientId, string? ClientSecret, string RedirectUri)
{
    /// Solo se usa el proveedor real cuando AMBAS credenciales están presentes.
    /// Si falta una, el sistema opera en modo simulado y NO debe pretender ser real.
    public bool TieneCredencialesReales =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);

    /// URL de autorización de LinkedIn. El `state` es obligatorio: es la defensa
    /// CSRF del flujo, y sin él el callback no puede distinguir una respuesta
    /// legítima de una forjada.
    public string ConstruirUrlDeAutorizacion(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("El flujo OAuth2 exige un `state` para la defensa CSRF.", nameof(state));
        if (!TieneCredencialesReales)
            throw new InvalidOperationException("No hay credenciales de LinkedIn configuradas.");

        return "https://www.linkedin.com/oauth/v2/authorization?response_type=code"
             + $"&client_id={Uri.EscapeDataString(ClientId!)}"
             + $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}"
             + $"&state={Uri.EscapeDataString(state)}"
             + "&scope=openid%20profile";
    }

    /// Destino al que se devuelve al usuario tras un callback exitoso. La sesión
    /// viaja en el FRAGMENTO: no se envía al servidor ni queda en logs ni en Referer.
    public static string UrlDeExito(string frontendUrl, string token)
        => $"{frontendUrl}/auth/callback#token={Uri.EscapeDataString(token)}";

    /// Destino tras un callback fallido: de vuelta al login, con el motivo y un
    /// mensaje para la persona, y SIN sesión.
    public static string UrlDeError(string frontendUrl, string motivo, string? mensaje)
        => $"{frontendUrl}/login?error={motivo}&message={Uri.EscapeDataString(mensaje ?? string.Empty)}";
}
