// Scaffold del backend .NET Core — Asistente de Empleabilidad IA.
// Minimal API con health check y un ping de wiring. Sin lógica de negocio:
// las capacidades se construyen por épica (EP-001 auth, EP-002 datos, ...).

var builder = WebApplication.CreateBuilder(args);

// CORS abierto SOLO para desarrollo local del SPA (se restringe en un slice posterior).
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

// Health check — usado por docker-compose y por la verificación del scaffold.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "backend-api" }));

// Ping de wiring: confirma que el SPA puede alcanzar la API.
app.MapGet("/api/ping", () => Results.Ok(new { message = "pong desde .NET Core", ts = DateTimeOffset.UtcNow }));

app.Run();
