namespace Empleabilidad.Api.Auth;

/// Provisión automática e idempotente de cuenta (HU-002).
/// Primer acceso crea el User_ID; acceso recurrente reutiliza el existente.
public class AccountProvisioningService
{
    private readonly IUserRepository _repo;

    public AccountProvisioningService(IUserRepository repo) => _repo = repo;

    public async Task<Guid> ProvisionAsync(string linkedinSub, string? name)
    {
        var existing = await _repo.FindByLinkedinSubAsync(linkedinSub);
        if (existing is not null)
            return existing.Value;

        return await _repo.CreateAsync(linkedinSub, name);
    }
}
