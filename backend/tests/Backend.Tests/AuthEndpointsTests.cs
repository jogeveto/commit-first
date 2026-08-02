using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Backend.Tests;

// Endpoints y configuración de seguridad del arranque (Program.cs), ejercitados
// en proceso con el servidor real. Existen porque una auditoría demostró que se
// podían romper garantías serias sin que ningún test fallara: ampliar la sesión a
// un año, desactivar la validación del emisor o abrir CORS a cualquier origen.
public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string Key = "dev-signing-key-cambiar-en-produccion-000000000000000000000000";

    public AuthEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static HttpClient SinRedirects(WebApplicationFactory<Program> f) =>
        f.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    // HU-001 AC1 — el inicio del flujo emite el `state` CSRF.
    [Fact]
    public async Task El_inicio_del_flujo_redirige_con_state()
    {
        var resp = await SinRedirects(_factory).GetAsync("/auth/linkedin/start");

        Assert.Equal(HttpStatusCode.Found, resp.StatusCode);
        Assert.Contains("state=", resp.Headers.Location!.ToString());
    }

    // HU-001 AC1 — el callback exitoso devuelve al SPA con la sesión en el FRAGMENTO.
    [Fact]
    public async Task El_callback_exitoso_devuelve_al_SPA_con_el_token_en_el_fragmento()
    {
        var cliente = SinRedirects(_factory);
        var destino = (await cliente.GetAsync("/auth/linkedin/start")).Headers.Location!.ToString();

        var resp = await cliente.GetAsync(destino);

        Assert.Equal(HttpStatusCode.Found, resp.StatusCode);
        var location = resp.Headers.Location!.ToString();
        Assert.Contains("/auth/callback#token=", location);
        Assert.DoesNotContain("?token=", location);
    }

    // HU-001 AC2/AC3 — los errores vuelven al login con el motivo y SIN sesión.
    [Fact]
    public async Task Un_callback_con_state_forjado_vuelve_al_login_sin_sesion()
    {
        var resp = await SinRedirects(_factory)
            .GetAsync("/auth/linkedin/callback?code=x&state=FORJADO");

        var location = resp.Headers.Location!.ToString();
        Assert.Contains("/login?error=invalid_state", location);
        Assert.DoesNotContain("token=", location);
    }

    // HU-003 — la sesión emitida dura 8 horas. Sin este assert se podía ampliar a
    // un año sin que nada fallara: una sesión robada seguiría viva indefinidamente.
    [Fact]
    public async Task La_sesion_emitida_caduca_a_las_8_horas()
    {
        var cliente = SinRedirects(_factory);
        var destino = (await cliente.GetAsync("/auth/linkedin/start")).Headers.Location!.ToString();
        var json = await cliente.GetStringAsync($"{destino}&format=json");
        var token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString()!;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(TimeSpan.FromHours(8), jwt.ValidTo - jwt.ValidFrom);
    }

    // HU-003 AC2 — un token bien firmado pero de OTRO emisor no vale. Sin este
    // assert se podía desactivar la validación del emisor sin que nada fallara.
    [Fact]
    public async Task Un_token_de_otro_emisor_es_rechazado()
    {
        var ajeno = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "emisor-ajeno",
            claims: new[] { new System.Security.Claims.Claim("sub", Guid.NewGuid().ToString()) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256)));

        var cliente = _factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", ajeno);

        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/me")).StatusCode);
    }

    // El endpoint de salud sigue respondiendo (lo usa el healthcheck de compose).
    [Fact]
    public async Task El_health_check_responde()
    {
        var resp = await _factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Contains("\"status\":\"ok\"", await resp.Content.ReadAsStringAsync());
    }
}
