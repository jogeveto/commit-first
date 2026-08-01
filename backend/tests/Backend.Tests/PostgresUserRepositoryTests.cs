using Empleabilidad.Api.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace Backend.Tests;

/// Pruebas de INTEGRACIÓN contra PostgreSQL real (HU-002). Existen porque la
/// invariante "el alta no deja cuenta parcial" NO puede demostrarse con un doble:
/// en un doble, quien decide si se escribe la fila es el propio doble, así que el
/// assert pasaría hiciera lo que hiciera el código de producción. Aquí la autoridad
/// es la base de datos.
///
/// Requieren el stack levantado y ejecutarse EN la red de compose:
///   docker run --rm --network my-top-profile_default \
///     -v "//c/.../backend://src" -w //src mcr.microsoft.com/dotnet/sdk:8.0 \
///     dotnet test tests/Backend.Tests/Backend.Tests.csproj
/// La cadena se toma de PG_TEST_CONN; por defecto apunta al servicio `db`.
public class PostgresUserRepositoryTests
{
    private static string Conn =>
        Environment.GetEnvironmentVariable("PG_TEST_CONN")
        ?? "Host=db;Port=5432;Database=empleabilidad;Username=empleo;Password=empleo_dev";

    // Identidad distinta por ejecución: las pruebas no dependen del estado dejado
    // por corridas anteriores (defecto que la auditoría encontró en el smoke).
    private static string NuevoSub(string caso) => $"test-{caso}-{Guid.NewGuid():N}";

    private static async Task<int> ContarAsync(string sub)
    {
        await using var conn = new NpgsqlConnection(Conn);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT count(*) FROM users WHERE linkedin_sub = @sub", conn);
        cmd.Parameters.AddWithValue("sub", sub);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // HU-002 AC1 — el primer acceso crea la fila de verdad, y el User_ID emitido es
    // el `id` real de esa fila (no un identificador inventado en memoria).
    [Fact]
    public async Task Primer_acceso_crea_la_fila_y_el_UserId_coincide_con_la_BD()
    {
        var sub = NuevoSub("alta");
        var repo = new PostgresUserRepository(Conn);

        var userId = await new AccountProvisioningService(repo).ProvisionAsync(sub, "Ana Torres");

        Assert.Equal(1, await ContarAsync(sub));
        Assert.Equal(userId, await repo.FindByLinkedinSubAsync(sub));
    }

    // HU-002 AC3 — dos primeros accesos SIMULTÁNEOS con la misma identidad. Es la
    // carrera check-then-act real (ambos ven "no existe" y ambos intentan crear):
    // el ON CONFLICT debe dejar exactamente una fila y un único User_ID.
    [Fact]
    public async Task Altas_concurrentes_del_mismo_sub_no_duplican_la_cuenta()
    {
        var sub = NuevoSub("carrera");
        var svc1 = new AccountProvisioningService(new PostgresUserRepository(Conn));
        var svc2 = new AccountProvisioningService(new PostgresUserRepository(Conn));

        var resultados = await Task.WhenAll(
            svc1.ProvisionAsync(sub, "Ana Torres"),
            svc2.ProvisionAsync(sub, "Ana Torres"));

        Assert.Equal(resultados[0], resultados[1]);
        Assert.Equal(1, await ContarAsync(sub));
    }

    // HU-002 AC2 — "sin cuenta parcial creada", verificado CONTRA LA BASE DE DATOS.
    // El fallo se provoca con una base inalcanzable; después se comprueba, con una
    // conexión sana e independiente, que no quedó ninguna fila para esa identidad.
    // A diferencia del test con doble, aquí quien responde es Postgres.
    [Fact]
    public async Task Fallo_al_persistir_no_deja_cuenta_en_la_base_de_datos()
    {
        var sub = NuevoSub("fallo");
        var repoRoto = new PostgresUserRepository(
            "Host=db;Port=5432;Database=base_inexistente;Username=empleo;Password=empleo_dev;Timeout=3");
        var svc = new LinkedInAuthenticationService(
            new AlwaysValidStateStore(),
            new FakeLinkedInClient(new LinkedInProfile(sub, "Ana Torres")),
            new AccountProvisioningService(repoRoto),
            new FakeJwtIssuer(),
            NullLogger<LinkedInAuthenticationService>.Instance);

        var result = await svc.HandleCallbackAsync("state-ok", "code-ok", null);

        Assert.Equal(AuthOutcome.ProvisioningFailed, result.Outcome);
        Assert.False(result.SessionEmitted);
        Assert.Equal(0, await ContarAsync(sub)); // la BD real no tiene cuenta parcial
    }

    /// Store que acepta cualquier `state`: en estos tests lo que se ejercita es la
    /// persistencia, no la guarda CSRF (que tiene sus propios tests).
    private sealed class AlwaysValidStateStore : IAuthStateStore
    {
        public string Issue() => "state-ok";
        public bool Validate(string? state) => true;
    }
}
