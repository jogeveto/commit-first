namespace Empleabilidad.Api.Auth;

/// Emisión de la sesión JWT ligada al User_ID (D4). El claim `sub` porta el
/// User_ID; el middleware de SS-001-b lo resolverá en cada petición protegida.
/// El emisor concreto (HS256 con JWT_SIGNING_KEY server-side) se construye al
/// cablear los endpoints; el dominio depende solo de esta abstracción.
public interface IJwtIssuer
{
    string Issue(Guid userId);
}
