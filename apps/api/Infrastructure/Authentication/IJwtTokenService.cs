namespace CaseManagement.Api.Infrastructure.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateAccessToken(Guid userId, string email, string fullName);
}