using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CaseManagement.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserIdOrNull(this ClaimsPrincipal? user)
    {
        var idString = user?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        return Guid.TryParse(idString, out var id) ? id : null;
    }

    public static Guid? GetActiveOrganizationIdOrNull(this ClaimsPrincipal? user)
    {
        var orgIdString = user?.FindFirstValue("active_organization_id")
            ?? user?.FindFirstValue("activeOrganizationId");
        return Guid.TryParse(orgIdString, out var orgId) ? orgId : null;
    }
}