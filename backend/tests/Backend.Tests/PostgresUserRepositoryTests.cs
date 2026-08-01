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

    // HU-002 AC2 — "no se emite sesión y no queda cuenta parcial", con el fallo
    // ocurriendo DENTRO de la misma base de datos y durante el alta real.
    //
    // Por qué así: una versión anterior de este test apuntaba el repositorio a otra
    // base de datos. Pasaba siempre —incluso con una implementación que insertara la
    // fila y luego fallara— porque el código nunca tocaba la base donde se contaba.
    // El fallo se inyecta ahora con un trigger en `users` que aborta el INSERT de
    // esta identidad concreta: es un error real de Postgres en el camino real.
    //
    // Mutación que lo pone en rojo: que CreateAsync trague el error de la base y
    // devuelva un Guid inventado (el flujo emitiría sesión para una cuenta que no
    // existe). Verificado ejecutándola.
    [Fact]
    public async Task Fallo_durante_el_alta_no_emite_sesion_ni_deja_cuenta()
    {
        var sub = NuevoSub("fallo");
        await EjecutarSqlAsync($@"
            CREATE OR REPLACE FUNCTION fallo_alta_{Sufijo(sub)}() RETURNS trigger AS $$
            BEGIN RAISE EXCEPTION 'fallo simulado durante el alta'; END;
            $$ LANGUAGE plpgsql;
            CREATE TRIGGER trg_fallo_{Sufijo(sub)} BEFORE INSERT ON users
            FOR EACH ROW WHEN (NEW.linkedin_sub = '{sub}')
            EXECUTE FUNCTION fallo_alta_{Sufijo(sub)}();");

        try
        {
            var svc = new LinkedInAuthenticationService(
                new AlwaysValidStateStore(),
                new FakeLinkedInClient(new LinkedInProfile(sub, "Ana Torres")),
                new AccountProvisioningService(new PostgresUserRepository(Conn)),
                new FakeJwtIssuer(),
                NullLogger<LinkedInAuthenticationService>.Instance);

            var result = await svc.HandleCallbackAsync("state-ok", "code-ok", null);

            Assert.Equal(AuthOutcome.ProvisioningFailed, result.Outcome);
            Assert.False(result.SessionEmitted);
            Assert.Null(result.Token);
            Assert.Equal(0, await ContarAsync(sub)); // sin cuenta parcial en la BD real
        }
        finally
        {
            await EjecutarSqlAsync(
                $"DROP TRIGGER IF EXISTS trg_fallo_{Sufijo(sub)} ON users; " +
                $"DROP FUNCTION IF EXISTS fallo_alta_{Sufijo(sub)}();");
        }
    }

    // Identificador SQL seguro derivado del sub (solo alfanumérico).
    private static string Sufijo(string sub) => sub.Replace("-", "")[^12..];

    private static async Task EjecutarSqlAsync(string sql)
    {
        await using var conn = new NpgsqlConnection(Conn);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    /// Store que acepta cualquier `state`: en estos tests lo que se ejercita es la
    /// persistencia, no la guarda CSRF (que tiene sus propios tests).
    private sealed class AlwaysValidStateStore : IAuthStateStore
    {
        public string Issue() => "state-ok";
        public bool Validate(string? state) => true;
    }
}
