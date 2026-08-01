using Empleabilidad.Api.Auth;
using Xunit;

namespace Backend.Tests;

// HU-002 — Provisión automática e idempotente de cuenta.
public class AccountProvisioningServiceTests
{
    // HU-002 AC1 — Primer acceso crea la cuenta.
    [Fact]
    public async Task Primer_acceso_crea_cuenta_y_retorna_UserId()
    {
        var repo = new InMemoryUserRepository();
        var svc = new AccountProvisioningService(repo);

        var userId = await svc.ProvisionAsync("linkedin-sub-123", "Ana Torres");

        Assert.NotEqual(Guid.Empty, userId);
        Assert.Equal(1, repo.Count);
    }

    // HU-002 AC3 — Acceso recurrente reutiliza el User_ID, no duplica.
    [Fact]
    public async Task Acceso_recurrente_reutiliza_UserId_y_no_duplica()
    {
        var repo = new InMemoryUserRepository();
        var svc = new AccountProvisioningService(repo);

        var primero = await svc.ProvisionAsync("linkedin-sub-xyz", "Ana Torres");
        var segundo = await svc.ProvisionAsync("linkedin-sub-xyz", "Ana Torres");

        Assert.Equal(primero, segundo);
        Assert.Equal(1, repo.Count);
    }
}
