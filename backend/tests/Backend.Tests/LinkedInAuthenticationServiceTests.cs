using Empleabilidad.Api.Auth;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Backend.Tests;

// HU-001 (login LinkedIn OAuth2) + HU-002 AC2 (fallo de alta) — orquestación del callback.
// El contrato fijado aquí define el orden de operaciones: la validación de `state`
// (CSRF) ocurre ANTES de cualquier otra cosa, y ningún camino de error emite sesión.
public class LinkedInAuthenticationServiceTests
{
    // Devuelve el servicio junto con los dobles, para poder assertar sobre ellos.
    private static (LinkedInAuthenticationService svc, FakeLinkedInClient linkedIn, RecordingLogger<LinkedInAuthenticationService> log)
        BuildService(IAuthStateStore stateStore, IUserRepository repo, LinkedInProfile? profile = null)
    {
        var linkedIn = new FakeLinkedInClient(
            profile ?? new LinkedInProfile("linkedin-sub-123", "Ana Torres"));
        var log = new RecordingLogger<LinkedInAuthenticationService>();
        var svc = new LinkedInAuthenticationService(
            stateStore, linkedIn, new AccountProvisioningService(repo), new FakeJwtIssuer(), log);
        return (svc, linkedIn, log);
    }

    // HU-001 AC1 — Happy path: state válido + code → sesión emitida ligada al User_ID.
    [Fact]
    public async Task Callback_con_state_valido_emite_sesion_ligada_al_UserId()
    {
        var store = new InMemoryAuthStateStore();
        var repo = new InMemoryUserRepository();
        var (svc, _, _) = BuildService(store, repo);
        var state = store.Issue();

        var result = await svc.HandleCallbackAsync(state, code: "auth-code-ok", error: null);

        Assert.Equal(AuthOutcome.Success, result.Outcome);
        Assert.True(result.SessionEmitted);
        Assert.NotNull(result.UserId);
        Assert.NotEqual(Guid.Empty, result.UserId!.Value);
        Assert.Equal($"jwt-for-{result.UserId}", result.Token);
        Assert.Equal(1, repo.Count);
    }

    // HU-001 AC2 — Consentimiento rechazado / error de LinkedIn → sin sesión, con mensaje.
    [Fact]
    public async Task Consentimiento_rechazado_no_emite_sesion()
    {
        var store = new InMemoryAuthStateStore();
        var repo = new InMemoryUserRepository();
        var (svc, _, _) = BuildService(store, repo);
        var state = store.Issue();

        var result = await svc.HandleCallbackAsync(state, code: null, error: "access_denied");

        Assert.Equal(AuthOutcome.ConsentRejected, result.Outcome);
        Assert.False(result.SessionEmitted);
        Assert.Null(result.Token);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Equal(0, repo.Count);
    }

    // HU-001 AC3 — state ausente/mismatch → rechazo, sin sesión (CSRF).
    [Fact]
    public async Task State_invalido_es_rechazado_sin_emitir_sesion()
    {
        var store = new InMemoryAuthStateStore();
        var repo = new InMemoryUserRepository();
        var (svc, _, _) = BuildService(store, repo);
        store.Issue(); // se emitió un state legítimo, pero el callback llega con otro

        var result = await svc.HandleCallbackAsync(
            state: "state-forjado", code: "auth-code-ok", error: null);

        Assert.Equal(AuthOutcome.InvalidState, result.Outcome);
        Assert.False(result.SessionEmitted);
        Assert.Equal(0, repo.Count);
    }

    // HU-001 AC3 — la guarda CSRF corre ANTES de canjear el `code`: un callback no
    // confiable no debe provocar ni una sola llamada a la frontera de LinkedIn.
    // (El assert sobre `repo.Count` no bastaba: probaba que no se provisionó, no que
    // no se hubiera hablado con el proveedor.)
    [Fact]
    public async Task State_invalido_no_llega_a_canjear_el_code_con_LinkedIn()
    {
        var store = new InMemoryAuthStateStore();
        var (svc, linkedIn, _) = BuildService(store, new InMemoryUserRepository());
        store.Issue();

        await svc.HandleCallbackAsync(state: "state-forjado", code: "auth-code-ok", error: null);

        Assert.Equal(0, linkedIn.ExchangeCount);
    }

    // HU-001 AC3 — "...y registra el intento". El rechazo por CSRF debe quedar
    // registrado como advertencia para que sea auditable.
    [Fact]
    public async Task State_invalido_registra_el_intento()
    {
        var store = new InMemoryAuthStateStore();
        var (svc, _, log) = BuildService(store, new InMemoryUserRepository());
        store.Issue();

        await svc.HandleCallbackAsync(state: "state-forjado", code: "auth-code-ok", error: null);

        var registro = Assert.Single(log.Entries.Where(e => e.Level == LogLevel.Warning));
        Assert.Contains("state", registro.Message, StringComparison.OrdinalIgnoreCase);
    }

    // El registro del intento NO debe volcar el `state` recibido: es un dato que
    // controla quien envía el callback y no aporta valor de auditoría.
    [Fact]
    public async Task El_registro_del_intento_no_vuelca_el_state_recibido()
    {
        var store = new InMemoryAuthStateStore();
        var (svc, _, log) = BuildService(store, new InMemoryUserRepository());
        store.Issue();

        await svc.HandleCallbackAsync(
            state: "valor-controlado-por-el-emisor", code: "auth-code-ok", error: null);

        Assert.DoesNotContain(log.Entries,
            e => e.Message.Contains("valor-controlado-por-el-emisor"));
    }

    // HU-002 AC2 (reacción de la orquestación) — si la persistencia falla: no se
    // emite sesión y hay un mensaje para el usuario.
    // La otra mitad del AC —"sin cuenta parcial creada"— NO se comprueba aquí: con
    // un doble sería un assert vacuo. Se verifica contra PostgreSQL real en
    // PostgresUserRepositoryTests.Fallo_al_persistir_no_deja_cuenta_en_la_base_de_datos.
    [Fact]
    public async Task Fallo_de_persistencia_no_emite_sesion()
    {
        var store = new InMemoryAuthStateStore();
        var (svc, _, _) = BuildService(store, new FailingUserRepository());
        var state = store.Issue();

        var result = await svc.HandleCallbackAsync(state, code: "auth-code-ok", error: null);

        Assert.Equal(AuthOutcome.ProvisioningFailed, result.Outcome);
        Assert.False(result.SessionEmitted);
        Assert.Null(result.Token);
        Assert.Null(result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }
}
