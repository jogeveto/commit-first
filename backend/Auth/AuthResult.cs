namespace Empleabilidad.Api.Auth;

/// Desenlace del flujo de callback OAuth2. Un único tipo de resultado para todos
/// los caminos (happy + errores) evita usar excepciones como control de flujo y
/// hace explícito, en el sistema de tipos, cada AC de HU-001/HU-002.
public enum AuthOutcome
{
    Success,            // HU-001 AC1 — autenticación exitosa
    ConsentRejected,    // HU-001 AC2 — consentimiento rechazado / error de LinkedIn
    InvalidState,       // HU-001 AC3 — state ausente/mismatch (CSRF)
    ProvisioningFailed  // HU-002 AC2 — fallo al persistir la cuenta
}

/// Resultado inmutable del callback. `SessionEmitted` es la invariante dura:
/// solo hay sesión cuando el desenlace es Success y hay token.
public record AuthResult(AuthOutcome Outcome, Guid? UserId, string? Token, string? Message)
{
    public bool SessionEmitted => Outcome == AuthOutcome.Success && Token is not null;
}
