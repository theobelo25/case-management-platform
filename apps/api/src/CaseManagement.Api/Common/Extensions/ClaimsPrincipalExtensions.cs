using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserIdOrNull(this ClaimsPrincipal? user)
    {
        var idString = user?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        return Guid.TryParse(idString, out var id) ? id : null;
    }
}