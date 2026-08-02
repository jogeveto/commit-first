using System.Net.Http.Json;
using System.Text.Json;

namespace Empleabilidad.Api.Auth;

/// Implementación de producción de la frontera LinkedIn (D1). Intercambia el `code`
/// por un access token (endpoint OAuth2 de LinkedIn) y consulta el perfil OIDC
/// (`/v2/userinfo`) para obtener `sub` + nombre. Se inyecta solo cuando
/// LINKEDIN_CLIENT_ID/SECRET están presentes.
///
/// VERIFICADO contra LinkedIn real (WC-002, 2026-08-01): con credenciales en `.env`
/// el flujo completo provisiona la cuenta con el `sub` OIDC auténtico. Lo que NO es
/// automatizable es la regresión de este camino: exige consentimiento humano en
/// LinkedIn, así que el contrato y el E2E corren contra FakeLinkedInClient y esta
/// clase queda cubierta solo por la verificación manual documentada.
public class RealLinkedInClient : ILinkedInClient
{
    private const string TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
    private const string UserInfoEndpoint = "https://api.linkedin.com/v2/userinfo";

    private readonly HttpClient _http;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    public RealLinkedInClient(HttpClient http, string clientId, string clientSecret, string redirectUri)
    {
        _http = http;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _redirectUri = redirectUri;
    }

    public async Task<LinkedInProfile> ExchangeCodeAsync(string code)
    {
        using var tokenResponse = await _http.PostAsync(TokenEndpoint, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _redirectUri,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            }));
        tokenResponse.EnsureSuccessStatusCode();

        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("Respuesta de token vacía de LinkedIn.");

        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
        userInfoRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
        using var userInfoResponse = await _http.SendAsync(userInfoRequest);
        userInfoResponse.EnsureSuccessStatusCode();

        var info = await userInfoResponse.Content.ReadFromJsonAsync<UserInfo>()
            ?? throw new InvalidOperationException("Perfil OIDC vacío de LinkedIn.");

        return new LinkedInProfile(info.Sub, info.Name);
    }

    private sealed record TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }

    private sealed record UserInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("sub")]
        public string Sub { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
