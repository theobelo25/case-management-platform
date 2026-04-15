using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CaseManagement.Application.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CaseManagement.Infrastructure.Auth;

public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options) : IAccessTokenIssuer
{
    private readonly JwtOptions _opt = options.Value;

    public string CreateAccessToken(Guid userId, string emailNormalized, string firstName, string lastName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_opt.AccessTokenLifetimeMinutes);

        var fullName = $"{firstName} {lastName}".Trim();
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, emailNormalized),
            new("given_name", firstName),
            new("family_name", lastName)
        };

        if (!string.IsNullOrWhiteSpace(fullName))
            claims.Add(new Claim("name", fullName));

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
