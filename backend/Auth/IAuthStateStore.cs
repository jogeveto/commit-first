namespace Empleabilidad.Api.Auth;

/// Emisión y validación del parámetro `state` OAuth2 (D2, protección CSRF).
/// Issue genera un `state` aleatorio y lo memoriza al iniciar el flujo;
/// Validate lo consume una sola vez en el callback (ausencia o mismatch → false).
public interface IAuthStateStore
{
    string Issue();
    bool Validate(string? state);
}
