// Backend .NET Core — Asistente de Empleabilidad IA.
// EP-001-a: identidad LinkedIn OAuth2 (HU-001) + provisión idempotente de cuenta
// (HU-002) sobre el scaffold containerizado. Mock-first: sin credenciales reales de
// LinkedIn se inyecta FakeLinkedInClient; la ruta real se activa por configuración.

using System.Security.Claims;
using System.Text;
using Empleabilidad.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// CORS: permisivo solo en desarrollo. Fuera de desarrollo se restringe al origen
// declarado del SPA (FRONTEND_URL) — el comentario "solo para desarrollo" no basta
// si el código no lo impone.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
    });
});

// --- Configuración (server-side; los secretos nunca llegan al frontend) ---
var config = builder.Configuration;
var connectionString = config.GetConnectionString("Default")
    ?? "Host=db;Port=5432;Database=empleabilidad;Username=empleo;Password=empleo_dev";
var jwtSigningKey = config["JWT_SIGNING_KEY"]
    ?? "dev-signing-key-cambiar-en-produccion-000000000000000000000000";
var jwtIssuer = config["JWT_ISSUER"] ?? "empleabilidad";
var callbackUri = config["LinkedIn:RedirectUri"] ?? "http://localhost:8080/auth/linkedin/callback";
// Origen del SPA al que se devuelve al usuario tras el callback (HU-001 AC1/AC2).
var frontendUrl = (config["FRONTEND_URL"] ?? "http://localhost:5173").TrimEnd('/');
var linkedInClientId = config["LINKEDIN_CLIENT_ID"];
var linkedInClientSecret = config["LINKEDIN_CLIENT_SECRET"];
var hasRealLinkedIn = !string.IsNullOrWhiteSpace(linkedInClientId)
                      && !string.IsNullOrWhiteSpace(linkedInClientSecret);

// --- Inyección de dependencias del flujo de autenticación ---
builder.Services.AddSingleton<IAuthStateStore, InMemoryAuthStateStore>();
builder.Services.AddSingleton<IJwtIssuer>(
    _ => new JwtIssuer(jwtSigningKey, jwtIssuer, TimeSpan.FromHours(8)));
builder.Services.AddScoped<IUserRepository>(_ => new PostgresUserRepository(connectionString));
builder.Services.AddScoped<AccountProvisioningService>();
builder.Services.AddScoped<LinkedInAuthenticationService>();

if (hasRealLinkedIn)
{
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<ILinkedInClient>(sp => new RealLinkedInClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        linkedInClientId!, linkedInClientSecret!, callbackUri));
}
else
{
    // Mock-first: comportamiento verificable sin credenciales (WC-002 pendiente).
    builder.Services.AddScoped<ILinkedInClient, FakeLinkedInClient>();
}

// --- HU-003 · Middleware de sesión JWT (design D4) ---
// Valida firma + emisor + expiración y proyecta el claim `sub` (User_ID) al contexto.
// ClockSkew=Zero: un token expirado es 401 de inmediato (sin la tolerancia de 5 min
// por defecto que enmascararía la expiración).
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conserva el claim `sub` sin remapear
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check — usado por docker-compose y por la verificación del scaffold.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "backend-api" }));
app.MapGet("/api/ping", () => Results.Ok(new { message = "pong desde .NET Core", ts = DateTimeOffset.UtcNow }));

// --- HU-001 · Inicio del flujo OAuth2 -------------------------------------------
// Genera el `state` (CSRF) y redirige a LinkedIn. En modo mock (sin credenciales)
// redirige directo al propio callback simulando el consentimiento, para que el
// journey camine end-to-end sin proveedor real.
app.MapGet("/auth/linkedin/start", (IAuthStateStore stateStore) =>
{
    var state = stateStore.Issue();

    if (hasRealLinkedIn)
    {
        var authorizeUrl =
            "https://www.linkedin.com/oauth/v2/authorization?response_type=code" +
            $"&client_id={Uri.EscapeDataString(linkedInClientId!)}" +
            $"&redirect_uri={Uri.EscapeDataString(callbackUri)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            "&scope=openid%20profile";
        return Results.Redirect(authorizeUrl);
    }

    var mockConsent = $"{callbackUri}?code=mock-auth-code&state={Uri.EscapeDataString(state)}";
    return Results.Redirect(mockConsent);
});

// --- HU-001 / HU-002 · Callback OAuth2 ------------------------------------------
// Valida `state`, intercambia el `code`, provisiona la cuenta y emite la sesión.
// El desenlace de AuthResult mapea al código HTTP y al cuerpo observable.
// Devuelve al usuario al SPA (HU-001 AC1 "me redirige autenticado a mi área";
// AC2 "vuelvo a la pantalla de login con un mensaje claro"). La sesión viaja en el
// FRAGMENTO de la URL (`#token=`): no se envía al servidor ni queda en logs de
// acceso ni en la cabecera Referer, a diferencia de un query string.
// El cliente `?format=json` conserva la respuesta JSON para verificación automatizada.
app.MapGet("/auth/linkedin/callback", async (
    string? state, string? code, string? error, string? format,
    LinkedInAuthenticationService auth) =>
{
    var result = await auth.HandleCallbackAsync(state, code, error);

    if (format == "json")
    {
        return result.Outcome switch
        {
            AuthOutcome.Success => Results.Ok(new
            {
                outcome = "success",
                userId = result.UserId,
                token = result.Token
            }),
            AuthOutcome.InvalidState => Results.Json(
                new { outcome = "invalid_state", message = result.Message },
                statusCode: StatusCodes.Status401Unauthorized),
            AuthOutcome.ConsentRejected => Results.Json(
                new { outcome = "consent_rejected", message = result.Message },
                statusCode: StatusCodes.Status401Unauthorized),
            AuthOutcome.ProvisioningFailed => Results.Json(
                new { outcome = "provisioning_failed", message = result.Message },
                statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    if (result.Outcome == AuthOutcome.Success)
        return Results.Redirect($"{frontendUrl}/auth/callback#token={Uri.EscapeDataString(result.Token!)}");

    var reason = result.Outcome switch
    {
        AuthOutcome.InvalidState => "invalid_state",
        AuthOutcome.ConsentRejected => "consent_rejected",
        AuthOutcome.ProvisioningFailed => "provisioning_failed",
        _ => "unknown"
    };
    return Results.Redirect(
        $"{frontendUrl}/login?error={reason}&message={Uri.EscapeDataString(result.Message ?? string.Empty)}");
});

// --- HU-003 · Endpoint protegido de prueba -------------------------------------
// Resuelve el User_ID del token; será la clave de tenancy que consumirá EP-002.
// Sin token / token inválido / expirado → 401 (lo aplica el middleware antes de entrar).
app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue("sub");
    return Results.Ok(new { userId });
}).RequireAuthorization();

app.Run();

// Expuesto para pruebas de integración (WebApplicationFactory) en slices posteriores.
public partial class Program { }
