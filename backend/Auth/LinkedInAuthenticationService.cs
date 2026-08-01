using Microsoft.Extensions.Logging;

namespace Empleabilidad.Api.Auth;

/// Orquesta el callback OAuth2 de LinkedIn (HU-001) sobre la provisión idempotente
/// de cuenta (HU-002). Es el corazón de EP-001-a: encadena la validación CSRF, el
/// intercambio del `code` por el perfil, el alta idempotente y la emisión de sesión.
///
/// El orden de las operaciones y la semántica de fallo (qué desenlace emite/no emite
/// sesión) se definen en HandleCallbackAsync — ver los tests en
/// LinkedInAuthenticationServiceTests para el contrato exacto.
public class LinkedInAuthenticationService
{
    private readonly IAuthStateStore _stateStore;
    private readonly ILinkedInClient _linkedIn;
    private readonly AccountProvisioningService _provisioning;
    private readonly IJwtIssuer _jwt;
    private readonly ILogger<LinkedInAuthenticationService> _log;

    public LinkedInAuthenticationService(
        IAuthStateStore stateStore,
        ILinkedInClient linkedIn,
        AccountProvisioningService provisioning,
        IJwtIssuer jwt,
        ILogger<LinkedInAuthenticationService> log)
    {
        _stateStore = stateStore;
        _linkedIn = linkedIn;
        _provisioning = provisioning;
        _jwt = jwt;
        _log = log;
    }

    /// Procesa el callback OAuth2. `state`/`code` vienen del query del callback;
    /// `error` está presente cuando LinkedIn rechaza el consentimiento.
    ///
    /// Orden deliberado: (1) la guarda CSRF corre PRIMERO — un callback con `state`
    /// no confiable se rechaza antes de tocar al proveedor o la BD; (2) el rechazo
    /// de consentimiento; (3) el camino feliz, donde un fallo de persistencia se
    /// aísla como ProvisioningFailed sin emitir sesión ni dejar cuenta parcial.
    public async Task<AuthResult> HandleCallbackAsync(string? state, string? code, string? error)
    {
        // (1) HU-001 AC3 — CSRF: state ausente/mismatch → rechazo, sin sesión, y se
        // registra el intento. El `state` recibido NO se vuelca en el registro: lo
        // controla quien envía el callback y no aporta valor de auditoría; basta con
        // dejar constancia de que hubo un rechazo y si el parámetro venía o no.
        if (!_stateStore.Validate(state))
        {
            _log.LogWarning(
                "Callback de LinkedIn rechazado: el parámetro state {StateStatus} la validación CSRF. No se emitió sesión.",
                state is null ? "no venía en" : "no superó");

            return new AuthResult(AuthOutcome.InvalidState, null, null,
                "La autenticación no se pudo verificar (state inválido). Intenta de nuevo.");
        }

        // (2) HU-001 AC2 — consentimiento rechazado / error de LinkedIn → sin sesión.
        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            return new AuthResult(AuthOutcome.ConsentRejected, null, null,
                "No se completó la autenticación con LinkedIn. Vuelve a intentarlo.");

        // (3) Intercambio del code por el perfil y provisión idempotente.
        var profile = await _linkedIn.ExchangeCodeAsync(code);

        Guid userId;
        try
        {
            userId = await _provisioning.ProvisionAsync(profile.Sub, profile.Name);
        }
        catch (Exception)
        {
            // HU-002 AC2 — fallo al persistir: sin sesión, sin cuenta parcial.
            return new AuthResult(AuthOutcome.ProvisioningFailed, null, null,
                "No se pudo completar el alta de tu cuenta. Intenta más tarde.");
        }

        // HU-001 AC1 / HU-002 AC1 — sesión emitida ligada al User_ID.
        var token = _jwt.Issue(userId);
        return new AuthResult(AuthOutcome.Success, userId, token, null);
    }
}
