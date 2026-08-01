using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Empleabilidad.Api.Auth;

/// Emisor concreto de sesión JWT (HS256, design D4). Firma con la clave server-side
/// (`JWT_SIGNING_KEY`), claim `sub = User_ID`, expiración configurable. La clave
/// nunca sale del backend; el frontend solo recibe el token ya firmado.
public class JwtIssuer : IJwtIssuer
{
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly TimeSpan _lifetime;

    public JwtIssuer(string signingKey, string issuer, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _issuer = issuer;
        _lifetime = lifetime;
    }

    public string Issue(Guid userId)
    {
        var now = DateTime.UtcNow;
        var expires = now.Add(_lifetime);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) },
            // `nbf` solo si queda antes de `exp`; para un lifetime no positivo se omite
            // (permite representar un token ya expirado sin violar nbf < exp).
            notBefore: now < expires ? now : null,
            expires: expires,
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
