using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Empleabilidad.Api.Auth;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Backend.Tests;

// HU-001 AC1 / HU-003 — la sesión emitida es un JWT firmado que porta el User_ID.
// Verifica el contrato de emisión (claim `sub`, firma válida); el middleware que la
// consume se construye en SS-001-b.
public class JwtIssuerTests
{
    private const string Key = "clave-de-firma-de-prueba-suficientemente-larga-1234567890";
    private const string Issuer = "empleabilidad";

    // El token emitido valida su firma y porta el User_ID en el claim `sub`.
    [Fact]
    public void Token_emitido_porta_UserId_en_sub_y_valida_firma()
    {
        var issuer = new JwtIssuer(Key, Issuer, TimeSpan.FromMinutes(60));
        var userId = Guid.NewGuid();

        var token = issuer.Issue(userId);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        }, out _);

        Assert.Equal(userId.ToString(), principal.FindFirst("sub")!.Value);
    }

    // Una firma con otra clave NO valida (protege la sesión de tokens forjados).
    [Fact]
    public void Token_firmado_con_otra_clave_no_valida()
    {
        var issuer = new JwtIssuer(Key, Issuer, TimeSpan.FromMinutes(60));
        var token = issuer.Issue(Guid.NewGuid());

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var otraClave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("clave-completamente-distinta-de-la-original-0987654321"));

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = otraClave
            }, out _));
    }
}
