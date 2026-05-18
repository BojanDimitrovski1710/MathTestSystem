using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MathTestSystem.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        _expiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");
    }

    public string GenerateToken(string userId, string username, string role)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_secret));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
