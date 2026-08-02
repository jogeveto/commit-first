using Empleabilidad.Api.Auth;
using Xunit;

namespace Backend.Tests;

// Decisiones del flujo OAuth2 que antes vivían dentro de Program.cs, fuera del
// alcance de cualquier test. Una auditoría demostró que se podían romper las dos
// más peligrosas con la suite entera en verde.
public class LinkedInOAuthOptionsTests
{
    private static LinkedInOAuthOptions Con(string? id, string? secret) =>
        new(id, secret, "http://localhost:8080/auth/linkedin/callback");

    // La decisión más peligrosa del sistema: usar el proveedor SIMULADO teniendo
    // credenciales reales significaría que cualquier `code` produce una identidad
    // válida — suplantación trivial en producción.
    [Fact]
    public void Con_ambas_credenciales_se_considera_configuracion_real()
    {
        Assert.True(Con("id", "secreto").TieneCredencialesReales);
    }

    [Theory]
    [InlineData(null, "secreto")]
    [InlineData("id", null)]
    [InlineData("", "secreto")]
    [InlineData("id", "  ")]
    [InlineData(null, null)]
    public void Sin_alguna_credencial_NO_se_considera_configuracion_real(string? id, string? secret)
    {
        Assert.False(Con(id, secret).TieneCredencialesReales);
    }

    // HU-001 AC3 — sin `state` no hay defensa CSRF. La URL debe llevarlo siempre.
    [Fact]
    public void La_url_de_autorizacion_lleva_el_state()
    {
        var url = Con("mi-id", "mi-secreto").ConstruirUrlDeAutorizacion("STATE-123");

        Assert.Contains("state=STATE-123", url);
    }

    [Fact]
    public void La_url_de_autorizacion_lleva_client_id_redirect_uri_y_scope()
    {
        var url = Con("mi-id", "mi-secreto").ConstruirUrlDeAutorizacion("STATE-123");

        Assert.StartsWith("https://www.linkedin.com/oauth/v2/authorization?response_type=code", url);
        Assert.Contains("client_id=mi-id", url);
        Assert.Contains("redirect_uri=http%3A%2F%2Flocalhost%3A8080%2Fauth%2Flinkedin%2Fcallback", url);
        Assert.Contains("scope=openid%20profile", url);
    }

    // Construir la URL sin `state` debe ser imposible, no silencioso.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construir_la_url_sin_state_es_un_error(string state)
    {
        Assert.Throws<ArgumentException>(() => Con("id", "secreto").ConstruirUrlDeAutorizacion(state));
    }

    [Fact]
    public void Construir_la_url_sin_credenciales_es_un_error()
    {
        Assert.Throws<InvalidOperationException>(
            () => Con(null, null).ConstruirUrlDeAutorizacion("STATE-123"));
    }

    // HU-001 AC1 — la sesión vuelve al SPA en el FRAGMENTO, nunca en el query:
    // así no viaja al servidor ni queda en logs de acceso ni en la cabecera Referer.
    [Fact]
    public void La_url_de_exito_lleva_el_token_en_el_fragmento()
    {
        var url = LinkedInOAuthOptions.UrlDeExito("http://localhost:5173", "jwt.de.prueba");

        Assert.Equal("http://localhost:5173/auth/callback#token=jwt.de.prueba", url);
        Assert.DoesNotContain("?token=", url);
    }

    // HU-001 AC2 — el error vuelve al login con motivo y mensaje, y SIN sesión.
    [Fact]
    public void La_url_de_error_vuelve_al_login_con_motivo_y_mensaje_y_sin_sesion()
    {
        var url = LinkedInOAuthOptions.UrlDeError(
            "http://localhost:5173", "consent_rejected", "No se completó la autenticación.");

        Assert.StartsWith("http://localhost:5173/login?error=consent_rejected", url);
        Assert.Contains("message=No%20se%20complet", url);
        Assert.DoesNotContain("token", url);
    }
}
