using Empleabilidad.Api.Auth;
using Xunit;

namespace Backend.Tests;

// Almacén de `state` OAuth2 (HU-001 AC3). Cubre la guarda CSRF y la caducidad.
// El design.md prometía un almacén "con TTL"; la implementación no lo tenía y los
// `state` de flujos abandonados se acumulaban sin límite.
public class InMemoryAuthStateStoreTests
{
    // Reloj controlado: permite comprobar la caducidad sin esperas reales.
    private sealed class Reloj
    {
        public DateTimeOffset Ahora = new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
        public DateTimeOffset Leer() => Ahora;
    }

    [Fact]
    public void Un_state_emitido_se_valida_una_sola_vez()
    {
        var store = new InMemoryAuthStateStore();
        var state = store.Issue();

        Assert.True(store.Validate(state));
        Assert.False(store.Validate(state)); // single-use: no se puede reutilizar
    }

    [Fact]
    public void Un_state_no_emitido_o_ausente_se_rechaza()
    {
        var store = new InMemoryAuthStateStore();

        Assert.False(store.Validate("forjado"));
        Assert.False(store.Validate(null));
    }

    // Mutación que lo mata: quitar la comprobación de caducidad en Validate.
    [Fact]
    public void Un_state_caducado_se_rechaza()
    {
        var reloj = new Reloj();
        var store = new InMemoryAuthStateStore(TimeSpan.FromMinutes(10), reloj.Leer);
        var state = store.Issue();

        reloj.Ahora = reloj.Ahora.AddMinutes(11);

        Assert.False(store.Validate(state));
    }

    // Mutación que lo mata: quitar PurgarCaducados de Issue. Sin purga, los flujos
    // abandonados se acumulan indefinidamente (crecimiento no acotado en memoria).
    [Fact]
    public void Los_states_abandonados_se_purgan_y_no_se_acumulan()
    {
        var reloj = new Reloj();
        var store = new InMemoryAuthStateStore(TimeSpan.FromMinutes(10), reloj.Leer);
        for (var i = 0; i < 50; i++) store.Issue(); // 50 flujos que nadie completa

        reloj.Ahora = reloj.Ahora.AddMinutes(11);
        store.Issue(); // un flujo nuevo dispara la purga de los caducados

        Assert.Equal(1, store.Pendientes);
    }

    // Un `state` todavía vigente no debe purgarse por error al emitir otro.
    [Fact]
    public void Emitir_un_state_nuevo_no_invalida_los_vigentes()
    {
        var reloj = new Reloj();
        var store = new InMemoryAuthStateStore(TimeSpan.FromMinutes(10), reloj.Leer);
        var primero = store.Issue();

        reloj.Ahora = reloj.Ahora.AddMinutes(5);
        store.Issue();

        Assert.True(store.Validate(primero));
    }
}
