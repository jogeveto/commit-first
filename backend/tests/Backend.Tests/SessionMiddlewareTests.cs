using System.Net;
using System.Net.Http.Headers;
using Empleabilidad.Api.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests;

// HU-003 — sesión ligada al User_ID. El middleware resuelve el User_ID del token en
// endpoints protegidos y responde 401 ante ausencia / manipulación / expiración.
// Pruebas de integración HTTP sobre el servidor real (WebApplicationFactory).
public class SessionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // Mismos parámetros que el default de dev de Program (misma clave/issuer → el
    // token emitido aquí es aceptado por el middleware del servidor bajo prueba).
    private const string Key = "dev-signing-key-cambiar-en-produccion-000000000000000000000000";
    private const string Issuer = "empleabilidad";

    public SessionMiddlewareTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static string Token(Guid userId, TimeSpan lifetime)
        => new JwtIssuer(Key, Issuer, lifetime).Issue(userId);

    // HU-003 AC1 — token válido → 200 y el endpoint resuelve el User_ID del claim.
    [Fact]
    public async Task Token_valido_resuelve_UserId_y_responde_200()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token(userId, TimeSpan.FromHours(1)));

        var resp = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Contains(userId.ToString(), await resp.Content.ReadAsStringAsync());
    }

    // HU-003 AC2 — token ausente → 401, sin ejecutar la operación.
    [Fact]
    public async Task Sin_token_responde_401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // HU-003 AC2 — token manipulado (firma rota) → 401.
    [Fact]
    public async Task Token_manipulado_responde_401()
    {
        var token = Token(Guid.NewGuid(), TimeSpan.FromHours(1));
        var tampered = token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tampered);

        var resp = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // HU-003 AC3 — token expirado → 401 (exige ClockSkew=0 en el middleware).
    [Fact]
    public async Task Token_expirado_responde_401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token(Guid.NewGuid(), TimeSpan.FromSeconds(-30)));

        var resp = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // HU-003 AC3 — "...con indicación de re-autenticar". La respuesta debe decirle
    // al cliente CÓMO seguir: el reto WWW-Authenticate con el esquema Bearer y el
    // motivo (token inválido/expirado). Sin este assert, el AC quedaba a medias
    // apoyado en un comportamiento por defecto del framework que nadie fijaba.
    [Fact]
    public async Task Token_expirado_indica_que_hay_que_re_autenticar()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token(Guid.NewGuid(), TimeSpan.FromSeconds(-30)));

        var resp = await client.GetAsync("/api/me");

        var reto = string.Join(" ", resp.Headers.WwwAuthenticate.Select(h => h.ToString()));
        Assert.Contains("Bearer", reto);
        Assert.Contains("invalid_token", reto);
        Assert.Contains("expired", reto, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await resp.Content.ReadAsStringAsync()); // no expone datos
    }

    // HU-003 AC2 — el 401 por token ausente también anuncia el esquema esperado.
    [Fact]
    public async Task Sin_token_anuncia_el_esquema_Bearer()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/me");

        Assert.Contains("Bearer",
            string.Join(" ", resp.Headers.WwwAuthenticate.Select(h => h.ToString())));
    }
}
