using Empleabilidad.Api.Auth;

namespace Backend.Tests;

/// Emisor de sesión determinista para tests. El JWT real (HS256 + middleware de
/// validación) se construye y verifica en SS-001-b; aquí solo importa el contrato
/// de la orquestación: cuando el flujo tiene éxito, hay un token ligado al User_ID.
public class FakeJwtIssuer : IJwtIssuer
{
    public string Issue(Guid userId) => $"jwt-for-{userId}";
}
