namespace Empleabilidad.Api.Auth;

/// Perfil mínimo devuelto por la frontera de LinkedIn (ILinkedInClient).
/// Sub = identificador estable del usuario en LinkedIn (clave natural de la cuenta).
public record LinkedInProfile(string Sub, string? Name);
