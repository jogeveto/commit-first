namespace Empleabilidad.Api.Auth;

/// Acceso a la tabla `users`. La lógica get-or-create (idempotencia) vive en
/// AccountProvisioningService; el repositorio solo busca y crea.
public interface IUserRepository
{
    Task<Guid?> FindByLinkedinSubAsync(string linkedinSub);
    Task<Guid> CreateAsync(string linkedinSub, string? name);
}
